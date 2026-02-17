using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.VisualBasic;
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
        // ACTION FILTER - SET UNREAD MESSAGE COUNT FOR SIDEBAR BADGE
        // ============================================================

       public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
{
    try
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            // ✅ Use proper unread count methods from service
            int unreadPrivate = await _chatService.GetUnreadMessageCountAsync(user.Id);
            int unreadGroup = await _groupChatService.GetUnreadGroupMessageCountAsync(user.Id);

            ViewBag.UnreadCount = unreadPrivate + unreadGroup;
        }
    }
    catch
    {
        ViewBag.UnreadCount = 0;
    }

    await next();
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

                // ============================================================
                // ✅ FIXED: CHECK FOR NEW MESSAGES EVERY TIME
                // No more "once per session" blocking!
                // ============================================================
                try
                {
                    // Get last seen timestamp
                    var lastSeenStr = HttpContext.Session.GetString($"LastSeen_{user.Id}");
                    var lastSeenDate = lastSeenStr != null
                        ? DateTime.Parse(lastSeenStr)
                        : DateTime.MinValue;


                    // ✅ UPDATED: Count only UNREAD messages
                    var conversations = await _chatService.GetUserConversationsAsync(user.Id);
                    int newPrivateMessages = 0;

                    foreach (var conv in conversations)
                    {
                        var unreadCount = await _chatService.GetUnreadMessageCountForConversationAsync(conv.Id, user.Id);
                        newPrivateMessages += unreadCount;
                    }

                    // ✅ UPDATED: Count only UNREAD group messages
                    var groupChats = await _groupChatService.GetUserGroupChatsAsync(user.Id);
                    int newGroupMessages = 0;

                    foreach (var group in groupChats)
                    {
                        var unreadCount = await _groupChatService.GetUnreadGroupMessageCountForChatAsync(group.GroupChatId, user.Id);
                        newGroupMessages += unreadCount;
                    }

                    int totalNew = newPrivateMessages + newGroupMessages;

                    // ✅ SHOW NOTIFICATION IF THERE ARE NEW MESSAGES (every time!)
                    if (totalNew > 0)
                    {
                        string messageText = "";

                        if (newPrivateMessages > 0 && newGroupMessages > 0)
                        {
                            messageText = $"You have {newPrivateMessages} new private message{(newPrivateMessages > 1 ? "s" : "")} and {newGroupMessages} new group message{(newGroupMessages > 1 ? "s" : "")}.";
                        }
                        else if (newPrivateMessages > 0)
                        {
                            messageText = $"You have {newPrivateMessages} new private message{(newPrivateMessages > 1 ? "s" : "")}.";
                        }
                        else
                        {
                            messageText = $"You have {newGroupMessages} new group chat message{(newGroupMessages > 1 ? "s" : "")}.";
                        }

                        NotificationHelper.ShowInfoWithLink(
                            this,
                            message: messageText,
                            linkUrl: "/Intern/AllChats",
                            linkText: "View Messages"
                        );
                    }
                }
                catch
                {
                    // If chat check fails, silently skip — don't break Dashboard
                }

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

        [HttpGet]
        public async Task<IActionResult> AllChats()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var intern = await _internService.GetInternByUserIdAsync(user.Id);

                var groupChats = await _groupChatService.GetUserGroupChatsAsync(user.Id);
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

                // Auto-inject supervisor if assigned but no messages yet
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

                // ✅ CRITICAL FIX: Mark all messages as "read" - update BOTH timestamps
                var now = DateTime.Now.ToString("o");
                HttpContext.Session.SetString($"LastVisitedMessages_{user.Id}", now);
                HttpContext.Session.SetString($"LastSeen_{user.Id}", now);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Messages(string userId = null)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (string.IsNullOrEmpty(userId))
                {
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

                    var model = new PrivateChatViewModel { Messages = allMessages };
                    return View(model);
                }
                else
                {
                    return RedirectToAction("PrivateChat", new { userId });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

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

                var conversation = await _chatService.GetOrCreatePrivateConversationAsync(currentUser.Id, userId);
                var messages = await _chatService.GetMessagesByConversationIdAsync(conversation.Id);

                // ✅ Mark messages as read
                await _chatService.MarkMessagesAsReadAsync(conversation.Id, currentUser.Id);

                // ✅ ADD THIS LINE: Pass conversation ID to view for SignalR
                ViewBag.ConversationId = conversation.Id;

                var roles = await _userManager.GetRolesAsync(otherUser);
                var role = roles.FirstOrDefault() ?? "User";

                var model = new PrivateChatViewModel
                {
                    OtherUserId = userId,
                    OtherUserName = otherUser.FullName,
                    OtherUserRole = role,
                    Messages = messages.Select(msg => new ChatMessageViewModel
                    {
                        ChatMessageId = msg.Id,
                        SenderId = msg.SenderId,
                        SenderName = msg.SenderId == currentUser.Id ? currentUser.FullName : otherUser.FullName,
                        ReceiverId = userId,
                        ReceiverName = otherUser.FullName,
                        MessageContent = msg.Content,
                        CreatedAt = msg.SentAt,
                        IsRead = msg.IsRead,
                        ReadAt = msg.ReadAt
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPrivateMessage(string receiverId, string messageContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(messageContent))
                {
                    NotificationHelper.ShowError(this, "Message cannot be empty.");
                    return RedirectToAction("PrivateChat", new { userId = receiverId });
                }

                var user = await _userManager.GetUserAsync(User);
                await _chatService.SendPrivateMessageAsync(user.Id, receiverId, messageContent);
                NotificationHelper.NotifyMessageSent(this);

                return RedirectToAction("PrivateChat", new { userId = receiverId });
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowError(this, GetFullErrorMessage(ex));
                return RedirectToAction("PrivateChat", new { userId = receiverId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GroupChat(int groupChatId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var isMember = await _groupChatService.IsMemberAsync(groupChatId, user.Id);
                if (!isMember)
                {
                    TempData["Error"] = "You are not a member of this group chat.";
                    return RedirectToAction("AllChats");
                }

                var groupChat = await _groupChatService.GetGroupChatByIdAsync(groupChatId);

                // ✅ NEW: Mark group chat as read
                await _groupChatService.MarkGroupChatAsReadAsync(groupChatId, user.Id);

                ViewBag.AvailableUsers = new List<object>();

                return View(groupChat);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("AllChats");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendGroupMessage(int groupChatId, string messageContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(messageContent))
                {
                    NotificationHelper.ShowError(this, "Message cannot be empty.");
                    return RedirectToAction("GroupChat", new { groupChatId });
                }

                var user = await _userManager.GetUserAsync(User);
                var dto = new SendGroupChatMessageDto { MessageContent = messageContent };

                await _groupChatService.SendMessageAsync(groupChatId, user.Id, dto);
                NotificationHelper.NotifyMessageSent(this);

                return RedirectToAction("GroupChat", new { groupChatId });
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowError(this, GetFullErrorMessage(ex));
                return RedirectToAction("GroupChat", new { groupChatId });
            }
        }

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

                var model = new SubmitDtrViewModel { RecordDate = DateTime.Today };
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
                    NotificationHelper.ShowError(this, "Supervisor not assigned yet. Cannot submit DTR.");
                    return RedirectToAction("Dashboard");
                }

                var dto = MappingHelper.MapDtrViewModelToDto(model);
                await _dtrService.SubmitDtrAsync(intern.InternId, dto);
                NotificationHelper.NotifyDtrSubmitted(this);

                return RedirectToAction("ViewDtrs");
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowError(this, GetFullErrorMessage(ex));
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
                    NotificationHelper.ShowError(this, "Supervisor not assigned yet. Cannot submit leave request.");
                    return RedirectToAction("Dashboard");
                }

                var dto = MappingHelper.MapLeaveRequestViewModelToDto(model);
                await _leaveRequestService.SubmitLeaveRequestAsync(intern.InternId, dto);
                NotificationHelper.NotifyLeaveSubmitted(this);

                return RedirectToAction("ViewLeaveRequests");
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowError(this, GetFullErrorMessage(ex));
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
                    return RedirectToAction("ViewEvaluation");
                }

                return View(evaluation);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ViewEvaluation");
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

                var intern = await _internService.GetInternByUserIdAsync(user.Id);
                if (intern == null)
                {
                    TempData["Error"] = "Intern profile not found.";
                    return RedirectToAction("Dashboard");
                }

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

                var intern = await _internService.GetInternByUserIdAsync(user.Id);
                if (intern == null)
                {
                    TempData["Error"] = "Intern profile not found.";
                    return RedirectToAction("Dashboard");
                }

                var pdfBytes = await _certificateService.GetCertificatePdfAsync(intern.InternId);
                var certificate = await _certificateService.GetCertificateByInternIdAsync(intern.InternId);

                if (certificate == null || pdfBytes == null || pdfBytes.Length == 0)
                {
                    TempData["Error"] = "Certificate not found or PDF is empty.";
                    return RedirectToAction("ViewCertificate");
                }

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