using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OJTManagementSystem.Dtos;
using OJTManagementSystem.Helpers;
using OJTManagementSystem.Models;
using OJTManagementSystem.Services.Interfaces;
using OJTManagementSystem.ViewModel;

namespace OJTManagementSystem.Controllers
{
    [Authorize(Roles = "Intern")]
    public class InternController : Controller
    {
        private readonly IInternService _internService;
        private readonly IDtrService _dtrService;
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly IEvaluationService _evaluationService;
        private readonly ICertificateService _certificateService;
        private readonly IChatService _chatService;
        private readonly IGroupChatService _groupChatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InternController(
            IInternService internService,
            IDtrService dtrService,
            ILeaveRequestService leaveRequestService,
            IEvaluationService evaluationService,
            ICertificateService certificateService,
            IChatService chatService,
            IGroupChatService groupChatService,
            UserManager<ApplicationUser> userManager)
        {
            _internService = internService;
            _dtrService = dtrService;
            _leaveRequestService = leaveRequestService;
            _evaluationService = evaluationService;
            _certificateService = certificateService;
            _chatService = chatService;
            _groupChatService = groupChatService;
            _userManager = userManager;
        }

        // ============================================================
        // DASHBOARD
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var intern = await _internService.GetInternByUserIdAsync(user.Id);

                if (intern == null)
                {
                    TempData["Error"] = "Intern profile not found.";
                    return RedirectToAction("Login", "Account");
                }

                var allDtrs = await _dtrService.GetInternDtrsAsync(intern.InternId);
                var allLeaves = await _leaveRequestService.GetInternLeaveRequestsAsync(intern.InternId);
                var evaluation = await _evaluationService.GetLatestEvaluationAsync(intern.InternId);
                var certificate = await _certificateService.GetCertificateByInternIdAsync(intern.InternId);

                var model = MappingHelper.CreateInternDashboardViewModel(
                    internProfile: intern,
                    allDtrs: allDtrs,
                    allLeaves: allLeaves,
                    latestEvaluation: evaluation,
                    certificate: certificate
                );

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return View(new InternDashboardViewModel());
            }
        }

        // ============================================================
        // CHAT & MESSAGING
        // ============================================================

        /// <summary>
        /// Display all chats (both private and group chats)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AllChats()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var intern = await _internService.GetInternByUserIdAsync(user.Id);

                // Get group chats
                var groupChats = await _groupChatService.GetUserGroupChatsAsync(user.Id);

                // Get existing private conversations
                var conversations = await _chatService.GetUserConversationsAsync(user.Id);
                var privateChats = new List<PrivateChatViewModel>();

                foreach (var conv in conversations)
                {
                    var otherUserId = conv.User1Id == user.Id ? conv.User2Id : conv.User1Id;
                    var otherUser = await _userManager.FindByIdAsync(otherUserId);
                    var messages = await _chatService.GetMessagesByConversationIdAsync(conv.Id);

                    var roles = await _userManager.GetRolesAsync(otherUser);
                    var role = roles.FirstOrDefault() ?? "User";

                    privateChats.Add(new PrivateChatViewModel
                    {
                        OtherUserId = otherUserId,
                        OtherUserName = otherUser.FullName,
                        OtherUserRole = role,
                        Messages = messages.Select(m => new ChatMessageViewModel
                        {
                            ChatMessageId = m.Id,
                            SenderId = m.SenderId,
                            SenderName = m.SenderId == user.Id ? user.FullName : otherUser.FullName,
                            ReceiverId = otherUserId,
                            ReceiverName = otherUser.FullName,
                            MessageContent = m.Content,
                            CreatedAt = m.SentAt,
                            IsRead = false
                        }).ToList()
                    });
                }

                // ✅ AUTO-INJECT SUPERVISOR: Show supervisor in private chats
                // as soon as they're assigned, even with 0 messages exchanged.
                if (intern != null && !string.IsNullOrEmpty(intern.SupervisorUserId))
                {
                    bool alreadyInList = privateChats.Any(p => p.OtherUserId == intern.SupervisorUserId);
                    if (!alreadyInList)
                    {
                        var supervisorUser = await _userManager.FindByIdAsync(intern.SupervisorUserId);
                        if (supervisorUser != null)
                        {
                            privateChats.Insert(0, new PrivateChatViewModel
                            {
                                OtherUserId = intern.SupervisorUserId,
                                OtherUserName = supervisorUser.FullName,
                                OtherUserRole = "Supervisor",
                                Messages = new List<ChatMessageViewModel>()
                            });
                        }
                    }
                }

                var model = new AllChatsViewModel
                {
                    GroupChats = groupChats,
                    PrivateChats = privateChats,
                    UnreadMessageCount = 0
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// Display messages (private chat)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Messages(string userId = null)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (string.IsNullOrEmpty(userId))
                {
                    // Show all conversations
                    var conversations = await _chatService.GetUserConversationsAsync(currentUser.Id);
                    var allMessages = new List<ChatMessageViewModel>();

                    foreach (var conv in conversations)
                    {
                        var messages = await _chatService.GetMessagesByConversationIdAsync(conv.Id);
                        var otherUserId = conv.User1Id == currentUser.Id ? conv.User2Id : conv.User1Id;
                        var otherUser = await _userManager.FindByIdAsync(otherUserId);

                        foreach (var msg in messages)
                        {
                            var sender = await _userManager.FindByIdAsync(msg.SenderId);
                            var receiverId = msg.SenderId == currentUser.Id ? otherUserId : currentUser.Id;
                            var receiver = await _userManager.FindByIdAsync(receiverId);

                            allMessages.Add(new ChatMessageViewModel
                            {
                                ChatMessageId = msg.Id,
                                SenderId = msg.SenderId,
                                SenderName = sender?.FullName,
                                ReceiverId = receiverId,
                                ReceiverName = receiver?.FullName,
                                MessageContent = msg.Content,
                                CreatedAt = msg.SentAt,
                                IsRead = false
                            });
                        }
                    }

                    var model = new PrivateChatViewModel
                    {
                        Messages = allMessages
                    };

                    return View(model);
                }
                else
                {
                    // Show specific conversation
                    return RedirectToAction("PrivateChat", new { userId });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// Display a specific private chat conversation
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PrivateChat(string userId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var otherUser = await _userManager.FindByIdAsync(userId);

                if (otherUser == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("AllChats");
                }

                // Get or create conversation
                var conversation = await _chatService.GetOrCreatePrivateConversationAsync(currentUser.Id, userId);
                var messages = await _chatService.GetMessagesByConversationIdAsync(conversation.Id);

                // Get role
                var roles = await _userManager.GetRolesAsync(otherUser);
                var role = roles.FirstOrDefault() ?? "User";

                var model = new PrivateChatViewModel
                {
                    OtherUserId = userId,
                    OtherUserName = otherUser.FullName,
                    OtherUserRole = role,
                    Messages = messages.Select(m => new ChatMessageViewModel
                    {
                        ChatMessageId = m.Id,
                        SenderId = m.SenderId,
                        SenderName = m.SenderId == currentUser.Id ? currentUser.FullName : otherUser.FullName,
                        ReceiverId = userId,
                        ReceiverName = otherUser.FullName,
                        MessageContent = m.Content,
                        CreatedAt = m.SentAt,
                        IsRead = false
                    }).ToList()
                };

                return View("Messages", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("AllChats");
            }
        }

        /// <summary>
        /// Send a private message
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPrivateMessage(string receiverId, string messageContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(messageContent))
                {
                    TempData["Error"] = "Message cannot be empty.";
                    return RedirectToAction("PrivateChat", new { userId = receiverId });
                }

                var user = await _userManager.GetUserAsync(User);
                await _chatService.SendPrivateMessageAsync(user.Id, receiverId, messageContent);

                TempData["Success"] = "Message sent successfully.";
                return RedirectToAction("PrivateChat", new { userId = receiverId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("PrivateChat", new { userId = receiverId });
            }
        }

     

        /// <summary>
        /// Display a specific group chat
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GroupChat(int groupChatId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                // Check if user is a member
                var isMember = await _groupChatService.IsMemberAsync(groupChatId, user.Id);
                if (!isMember)
                {
                    TempData["Error"] = "You are not a member of this group chat.";
                    return RedirectToAction("AllChats");
                }

                var groupChat = await _groupChatService.GetGroupChatByIdAsync(groupChatId);

                // Interns don't add members, so we pass empty list
                ViewBag.AvailableUsers = new List<object>();

                return View(groupChat);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("AllChats");
            }
        }

        /// <summary>
        /// Send a message in a group chat
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendGroupMessage(int groupChatId, string messageContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(messageContent))
                {
                    TempData["Error"] = "Message cannot be empty.";
                    return RedirectToAction("GroupChat", new { groupChatId });
                }

                var user = await _userManager.GetUserAsync(User);
                var dto = new SendGroupChatMessageDto { MessageContent = messageContent };

                await _groupChatService.SendMessageAsync(groupChatId, user.Id, dto);

                TempData["Success"] = "Message sent successfully.";
                return RedirectToAction("GroupChat", new { groupChatId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("GroupChat", new { groupChatId });
            }
        }

        /// <summary>
        /// Add a member to a group chat
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGroupChatMember(int groupChatId, string userId)
        {
            try
            {
                await _groupChatService.AddMemberAsync(groupChatId, userId);
                TempData["Success"] = "Member added successfully!";
                return RedirectToAction("GroupChat", new { groupChatId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("GroupChat", new { groupChatId });
            }
        }

        // ============================================================
        // DAILY TIME RECORD (DTR)
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> SubmitDtr()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var intern = await _internService.GetInternByUserIdAsync(user.Id);

                if (intern == null || intern.SupervisorId == null)
                {
                    TempData["Error"] = "You cannot submit a DTR yet. Please wait for a supervisor to be assigned.";
                    return RedirectToAction("Dashboard");
                }

                var model = new SubmitDtrViewModel
                {
                    RecordDate = DateTime.Today
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitDtr(SubmitDtrViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                if (model.TimeOut <= model.TimeIn)
                {
                    ModelState.AddModelError("TimeOut", "Time Out must be after Time In");
                    return View(model);
                }

                if (model.RecordDate > DateTime.Today)
                {
                    ModelState.AddModelError("RecordDate", "Cannot submit DTR for future dates");
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                var intern = await _internService.GetInternByUserIdAsync(user.Id);

                if (intern.SupervisorId == null)
                {
                    TempData["Error"] = "Supervisor not assigned yet. Cannot submit DTR.";
                    return RedirectToAction("Dashboard");
                }

                var dto = MappingHelper.MapDtrViewModelToDto(model);
                await _dtrService.SubmitDtrAsync(intern.InternId, dto);

                TempData["Success"] = "DTR submitted successfully.";
                return RedirectToAction("ViewDtrs");
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewDtrs()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var intern = await _internService.GetInternByUserIdAsync(user.Id);
                var dtrs = await _dtrService.GetInternDtrsAsync(intern.InternId);

                return View(dtrs);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewDtrDetails(int dtrId)
        {
            try
            {
                var dtr = await _dtrService.GetDtrByIdAsync(dtrId);

                if (dtr == null)
                {
                    TempData["Error"] = "DTR not found.";
                    return RedirectToAction("ViewDtrs");
                }

                return View(dtr);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ViewDtrs");
            }
        }

        // ============================================================
        // LEAVE REQUESTS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> RequestLeave()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var intern = await _internService.GetInternByUserIdAsync(user.Id);

                if (intern == null || intern.SupervisorId == null)
                {
                    TempData["Error"] = "Supervisor not assigned yet. Cannot submit leave request.";
                    return RedirectToAction("Dashboard");
                }

                var model = new SubmitLeaveRequestViewModel
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestLeave(SubmitLeaveRequestViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                if (model.EndDate < model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after or equal to start date");
                    return View(model);
                }

                if (model.StartDate < DateTime.Today)
                {
                    ModelState.AddModelError("StartDate", "Cannot submit leave request for past dates");
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                var intern = await _internService.GetInternByUserIdAsync(user.Id);

                if (intern.SupervisorId == null)
                {
                    TempData["Error"] = "Supervisor not assigned yet. Cannot submit leave request.";
                    return RedirectToAction("Dashboard");
                }

                var dto = MappingHelper.MapLeaveRequestViewModelToDto(model);
                await _leaveRequestService.SubmitLeaveRequestAsync(intern.InternId, dto);

                TempData["Success"] = "Leave request submitted successfully.";
                return RedirectToAction("ViewLeaveRequests");
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewLeaveRequests()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var intern = await _internService.GetInternByUserIdAsync(user.Id);
                var leaveRequests = await _leaveRequestService.GetInternLeaveRequestsAsync(intern.InternId);

                return View(leaveRequests);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewLeaveDetails(int leaveRequestId)
        {
            try
            {
                var leave = await _leaveRequestService.GetLeaveRequestByIdAsync(leaveRequestId);

                if (leave == null)
                {
                    TempData["Error"] = "Leave request not found.";
                    return RedirectToAction("ViewLeaveRequests");
                }

                return View(leave);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ViewLeaveRequests");
            }
        }

        // ============================================================
        // EVALUATIONS
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ViewEvaluation()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var intern = await _internService.GetInternByUserIdAsync(user.Id);

                if (intern == null)
                {
                    TempData["Error"] = "Intern not found.";
                    return RedirectToAction("Dashboard");
                }

                var evaluations = await _evaluationService.GetInternEvaluationsAsync(intern.InternId);

                return View(evaluations);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewEvaluationDetails(int evaluationId)
        {
            try
            {
                var evaluation = await _evaluationService.GetEvaluationByIdAsync(evaluationId);

                if (evaluation == null)
                {
                    TempData["Error"] = "Evaluation not found.";
                    return RedirectToAction("ViewEvaluation"); // ✅ FIXED HERE
                }

                return View(evaluation);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ViewEvaluation"); // ✅ FIXED HERE
            }
        }

        // ============================================================
        // CERTIFICATE
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ViewCertificate()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Dashboard");
                }

                // Get intern profile
                var intern = await _internService.GetInternByUserIdAsync(user.Id);
                if (intern == null)
                {
                    TempData["Error"] = "Intern profile not found.";
                    return RedirectToAction("Dashboard");
                }

                // Get certificate
                var certificate = await _certificateService.GetCertificateByInternIdAsync(intern.InternId);

                if (certificate == null)
                {
                    TempData["Info"] = "No certificate issued yet. Your certificate will be available once your OJT is completed and your supervisor generates it.";
                    return RedirectToAction("Dashboard");
                }

                return View(certificate);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading certificate: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// Download intern's certificate as PDF
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadCertificate()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Dashboard");
                }

                // Get intern profile
                var intern = await _internService.GetInternByUserIdAsync(user.Id);
                if (intern == null)
                {
                    TempData["Error"] = "Intern profile not found.";
                    return RedirectToAction("Dashboard");
                }

                // Get PDF bytes
                var pdfBytes = await _certificateService.GetCertificatePdfAsync(intern.InternId);
                var certificate = await _certificateService.GetCertificateByInternIdAsync(intern.InternId);

                if (certificate == null || pdfBytes == null || pdfBytes.Length == 0)
                {
                    TempData["Error"] = "Certificate not found or PDF is empty.";
                    return RedirectToAction("ViewCertificate");
                }

                // Return PDF file for download
                return File(pdfBytes, "application/pdf", certificate.PdfFileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error downloading certificate: {ex.Message}";
                return RedirectToAction("ViewCertificate");
            }
        }

        // ============================================================
        // PROFILE
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var intern = await _internService.GetInternByUserIdAsync(user.Id);

                if (intern == null)
                {
                    TempData["Error"] = "Profile not found.";
                    return RedirectToAction("Dashboard");
                }

                return View(intern);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        private string GetFullErrorMessage(Exception ex)
        {
            var messages = new List<string>();
            while (ex != null)
            {
                messages.Add(ex.Message);
                ex = ex.InnerException;
            }
            return string.Join(" → ", messages);
        }
    }
}