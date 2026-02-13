using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OJTManagementSystem.Dtos;
using OJTManagementSystem.Enums;
using OJTManagementSystem.Helpers;
using OJTManagementSystem.Models;
using OJTManagementSystem.Services.Interfaces;
using OJTManagementSystem.ViewModel;

namespace OJTManagementSystem.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly IInternService _internService;
        private readonly IDtrService _dtrService;
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly IEvaluationService _evaluationService;
        private readonly ICertificateService _certificateService;
        private readonly IChatService _chatService;
        private readonly IGroupChatService _groupChatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupervisorController(
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
        // HELPER METHODS
        // ============================================================

        private async Task<Supervisor> GetCurrentSupervisorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return null;

            return await _internService.GetSupervisorByUserIdAsync(user.Id);
        }

        private async Task<Supervisor> GetOrCreateCurrentSupervisorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return null;

            return await _internService.FindOrCreateSupervisorAsync(user.Id);
        }

        // ============================================================
        // DASHBOARD
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var supervisor = await GetOrCreateCurrentSupervisorAsync();

                var myInterns = new List<InternViewModel>();
                var myInternIds = new List<int>();

                if (supervisor != null)
                {
                    myInterns = (await _internService.GetSupervisorInternsAsync(supervisor.SupervisorId)).ToList();
                    myInternIds = myInterns.Select(i => i.InternId).ToList();
                }

                var allPendingDtrs = await _dtrService.GetPendingDtrsAsync();
                var allPendingLeaves = await _leaveRequestService.GetPendingLeaveRequestsAsync();

                var myPendingDtrs = allPendingDtrs
                    .Where(d => myInternIds.Contains(d.InternId))
                    .ToList();

                var myPendingLeaves = allPendingLeaves
                    .Where(l => myInternIds.Contains(l.InternId))
                    .ToList();

                var model = new SupervisorDashboardViewModel
                {
                    InternCount = myInterns.Count,
                    PendingDtrCount = myPendingDtrs.Count,
                    PendingLeaveRequestCount = myPendingLeaves.Count,
                    PendingEvaluationCount = 0,
                    PendingDtrs = myPendingDtrs.Take(5).ToList(),
                    PendingLeaveRequests = myPendingLeaves.Take(5).ToList(),
                    Interns = myInterns
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return View(new SupervisorDashboardViewModel());
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

                // Get group chats
                var groupChats = await _groupChatService.GetUserGroupChatsAsync(user.Id);

                // Get private conversations
                var conversations = await _chatService.GetUserConversationsAsync(user.Id);
                var privateChats = new List<PrivateChatViewModel>();

                foreach (var conv in conversations)
                {
                    var otherUserId = conv.User1Id == user.Id ? conv.User2Id : conv.User1Id;
                    var otherUser = await _userManager.FindByIdAsync(otherUserId);
                    var messages = await _chatService.GetMessagesByConversationIdAsync(conv.Id);

                    // Determine role
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
        /// Display create group chat form
        /// </summary>
        [HttpGet]
        public IActionResult CreateGroupChat()
        {
            return View(new CreateGroupChatViewModel());
        }

        /// <summary>
        /// Create a new group chat (e.g., for all assigned interns)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroupChat(CreateGroupChatViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                var user = await _userManager.GetUserAsync(User);
                var dto = MappingHelper.MapCreateGroupChatViewModelToDto(viewModel);
                var groupChat = await _groupChatService.CreateGroupChatAsync(user.Id, dto);

                TempData["Success"] = "Group chat created successfully!";
                return RedirectToAction("GroupChat", new { groupChatId = groupChat.GroupChatId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return View(viewModel);
            }
        }

        /// <summary>
        /// Create a group chat with all assigned interns automatically
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroupChatWithInterns()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var supervisor = await GetCurrentSupervisorAsync();

                if (supervisor == null)
                {
                    TempData["Error"] = "Supervisor profile not found.";
                    return RedirectToAction("Dashboard");
                }

                // Get all interns assigned to this supervisor
                var myInterns = await _internService.GetSupervisorInternsAsync(supervisor.SupervisorId);

                if (!myInterns.Any())
                {
                    TempData["Error"] = "You don't have any assigned interns yet.";
                    return RedirectToAction("ManageInterns");
                }

                // Create group chat
                var dto = new CreateGroupChatDto
                {
                    GroupName = $"{user.FullName}'s Team",
                    Description = "Group chat for all assigned interns and supervisor"
                };

                var groupChat = await _groupChatService.CreateGroupChatAsync(user.Id, dto);

                // Add all interns to the group
                foreach (var intern in myInterns)
                {
                    try
                    {
                        await _groupChatService.AddMemberAsync(groupChat.GroupChatId, intern.UserId);
                    }
                    catch
                    {
                        // Continue if an intern can't be added
                    }
                }

                TempData["Success"] = $"Team group chat created with {myInterns.Count()} intern(s)!";
                return RedirectToAction("GroupChat", new { groupChatId = groupChat.GroupChatId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ManageInterns");
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

                // ═══════════════════════════════════════════════════════════
                // ✅ ADD THIS ENTIRE SECTION - Get available interns
                // ═══════════════════════════════════════════════════════════

                try
                {
                    // Get current supervisor
                    var supervisor = await GetCurrentSupervisorAsync();

                    if (supervisor != null)
                    {
                        // Get all assigned interns for this supervisor
                        var myInterns = await _internService.GetSupervisorInternsAsync(supervisor.SupervisorId);

                        // Get current members' UserIds
                        var currentMemberIds = groupChat.Members.Select(m => m.UserId).ToList();

                        // Filter out interns who are already members
                        var availableInterns = myInterns
                            .Where(i => !currentMemberIds.Contains(i.UserId))
                            .ToList();

                        // Pass available interns to view via ViewBag
                        ViewBag.AvailableUsers = availableInterns.Select(i => new
                        {
                            UserId = i.UserId,
                            FullName = $"{i.FirstName} {i.LastName}",
                            Email = i.Email
                        }).ToList();
                    }
                    else
                    {
                        // If supervisor is null, no interns available
                        ViewBag.AvailableUsers = new List<object>();
                    }
                }
                catch
                {
                    // If there's an error getting interns, set empty list
                    ViewBag.AvailableUsers = new List<object>();
                }

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
        // MANAGE INTERNS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ManageInterns()
        {
            try
            {
                var supervisor = await GetOrCreateCurrentSupervisorAsync();
                List<InternViewModel> interns = new List<InternViewModel>();

                if (supervisor != null && supervisor.SupervisorId > 0)
                {
                    var supervisorInterns = await _internService.GetSupervisorInternsAsync(supervisor.SupervisorId);
                    interns = (supervisorInterns ?? new List<InternViewModel>()).ToList();
                }
                else
                {
                    interns = new List<InternViewModel>();
                }

                return View(interns);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }


        [HttpGet]
        public async Task<IActionResult> AvailableInterns()
        {
            try
            {
                var availableInterns = await _internService.GetAvailableInternsAsync();
                return View(availableInterns);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ManageInterns");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignIntern(int internId)
        {
            try
            {
                var supervisor = await GetOrCreateCurrentSupervisorAsync();
                if (supervisor == null)
                {
                    TempData["Error"] = "Unable to identify your account. Please log out and log back in.";
                    return RedirectToAction("AvailableInterns");
                }

                await _internService.AssignInternToSupervisorAsync(internId, supervisor.SupervisorId);
                TempData["Success"] = "Intern successfully assigned to you.";
                return RedirectToAction("ManageInterns");
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("AvailableInterns");
            }
        }

        // ============================================================
        // DTR MANAGEMENT
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ViewPendingDtrs()
        {
            try
            {
                var supervisor = await GetCurrentSupervisorAsync();
                if (supervisor == null)
                {
                    var allPendingDtrs = await _dtrService.GetPendingDtrsAsync();
                    return View(allPendingDtrs);
                }

                var myInterns = await _internService.GetSupervisorInternsAsync(supervisor.SupervisorId);
                var myInternIds = myInterns.Select(i => i.InternId).ToList();

                var allDtrs = await _dtrService.GetPendingDtrsAsync();
                var myPendingDtrs = allDtrs.Where(d => myInternIds.Contains(d.InternId)).ToList();

                return View(myPendingDtrs);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ApproveDtr(int dtrId)
        {
            try
            {
                var dtr = await _dtrService.GetDtrByIdAsync(dtrId);
                if (dtr == null)
                {
                    TempData["Error"] = "DTR not found.";
                    return RedirectToAction("ViewPendingDtrs");
                }

                var model = MappingHelper.MapDtrViewModelToApprovalViewModel(dtr);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ViewPendingDtrs");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDtr(ApproveDtrViewModel model)
        {
            try
            {
                if (model.IsApproved)
                {
                    ModelState.Remove("RejectionReason");
                }
                else
                {
                    ModelState.Remove("ApprovedBy");

                    if (string.IsNullOrWhiteSpace(model.RejectionReason))
                    {
                        ModelState.AddModelError("RejectionReason", "Rejection reason is required when rejecting a DTR.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    TempData["Error"] = $"Validation Error: {errors}";
                    return View(model);
                }

                var dto = MappingHelper.MapApproveDtrViewModelToDto(model);
                await _dtrService.ApproveDtrAsync(dto);

                TempData["Success"] = $"DTR {(model.IsApproved ? "approved" : "rejected")} successfully.";
                return RedirectToAction("ViewPendingDtrs");
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return View(model);
            }
        }


        // ============================================================
        // LEAVE REQUEST MANAGEMENT
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ViewPendingLeaves()
        {
            try
            {
                var supervisor = await GetCurrentSupervisorAsync();
                if (supervisor == null)
                {
                    var allPendingLeaves = await _leaveRequestService.GetPendingLeaveRequestsAsync();
                    return View(allPendingLeaves);
                }

                var myInterns = await _internService.GetSupervisorInternsAsync(supervisor.SupervisorId);
                var myInternIds = myInterns.Select(i => i.InternId).ToList();

                var allLeaves = await _leaveRequestService.GetPendingLeaveRequestsAsync();
                var myPendingLeaves = allLeaves.Where(l => myInternIds.Contains(l.InternId)).ToList();

                return View(myPendingLeaves);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ApproveLeaveRequest(int leaveRequestId)
        {
            try
            {
                var leave = await _leaveRequestService.GetLeaveRequestByIdAsync(leaveRequestId);
                if (leave == null)
                {
                    TempData["Error"] = "Leave request not found.";
                    return RedirectToAction("ViewPendingLeaves");
                }

                var model = MappingHelper.MapLeaveRequestViewModelToApprovalViewModel(leave);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ViewPendingLeaves");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLeaveRequest(ApproveLeaveRequestViewModel model)
        {
            try
            {
                if (model.Status == LeaveStatus.Approved)
                    ModelState.Remove("RejectionReason");
                else if (model.Status == LeaveStatus.Rejected)
                    ModelState.Remove("ApprovedBy");

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    TempData["Error"] = $"Validation Error: {errors}";
                    return View(model);
                }

                if (model.Status == LeaveStatus.Rejected && string.IsNullOrWhiteSpace(model.RejectionReason))
                {
                    ModelState.AddModelError("RejectionReason", "Rejection reason is required when rejecting a leave request");
                    return View(model);
                }

                if (model.Status == LeaveStatus.Approved && string.IsNullOrWhiteSpace(model.ApprovedBy))
                {
                    var user = await _userManager.GetUserAsync(User);
                    model.ApprovedBy = user?.FullName ?? "Supervisor";
                }

                var dto = MappingHelper.MapApproveLeaveRequestViewModelToDto(model);
                await _leaveRequestService.ApproveLeaveRequestAsync(dto);

                TempData["Success"] = $"Leave request {(model.Status == LeaveStatus.Approved ? "approved" : "rejected")} successfully.";
                return RedirectToAction("ViewPendingLeaves");
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return View(model);
            }
        }

        // ============================================================
        // EVALUATION
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> EvaluateIntern(int internId)
        {
            try
            {
                var intern = await _internService.GetInternByIdAsync(internId);
                if (intern == null)
                {
                    TempData["Error"] = "Intern not found.";
                    return RedirectToAction("ManageInterns");
                }

                var model = new EvaluateInternViewModel
                {
                    InternId = internId
                };

                ViewBag.InternName = intern.FullName;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ManageInterns");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EvaluateIntern(EvaluateInternViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    TempData["Error"] = $"Validation Error: {errors}";
                    return View(model);
                }

                var supervisor = await GetCurrentSupervisorAsync();
                if (supervisor == null)
                {
                    TempData["Error"] = "Supervisor profile not found.";
                    return RedirectToAction("ManageInterns");
                }

                var dto = MappingHelper.MapEvaluateInternViewModelToDto(model);
                await _evaluationService.SubmitEvaluationAsync(supervisor.SupervisorId, dto);

                TempData["Success"] = "Evaluation submitted successfully.";
                return RedirectToAction("ManageInterns");
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewEvaluations(int internId)
        {
            try
            {
                var evaluations = await _evaluationService.GetInternEvaluationsAsync(internId);
                var intern = await _internService.GetInternByIdAsync(internId);

                ViewBag.InternName = intern?.FullName ?? "Unknown";
                return View(evaluations);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ManageInterns");
            }
        }

        // ============================================================
        // CERTIFICATES
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GenerateCertificate(int internId)
        {
            try
            {
                var intern = await _internService.GetInternByIdAsync(internId);
                if (intern == null)
                {
                    TempData["Error"] = "Intern not found.";
                    return RedirectToAction("ManageInterns");
                }

                var user = await _userManager.GetUserAsync(User);
                var issuedBy = user?.FullName ?? "Supervisor";

                var certificate = await _certificateService.GenerateCertificateAsync(internId, issuedBy);

                TempData["Success"] = $"Certificate generated successfully for {intern.FullName}.";
                return RedirectToAction("ViewCertificate", new { internId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ManageInterns");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewCertificate(int internId)
        {
            try
            {
                var certificate = await _certificateService.GetCertificateByInternIdAsync(internId);
                if (certificate == null)
                {
                    TempData["Error"] = "No certificate found for this intern.";
                    return RedirectToAction("ManageInterns");
                }

                return View(certificate);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ManageInterns");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadCertificate(int internId)
        {
            try
            {
                var pdfBytes = await _certificateService.GetCertificatePdfAsync(internId);
                var certificate = await _certificateService.GetCertificateByInternIdAsync(internId);

                return File(pdfBytes, "application/pdf", certificate.PdfFileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("ManageInterns");
            }
        }

        // ============================================================
        // DELETE GROUP CHAT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroupChat(int groupChatId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var groupChat = await _groupChatService.GetGroupChatByIdAsync(groupChatId);

                if (groupChat == null)
                {
                    TempData["Error"] = "Group chat not found.";
                    return RedirectToAction("AllChats");
                }

                if (groupChat.CreatorId != user.Id)
                {
                    TempData["Error"] = "You can only delete group chats that you created.";
                    return RedirectToAction("AllChats");
                }

                await _groupChatService.DeleteGroupChatAsync(groupChatId, user.Id);
                TempData["Success"] = $"Group chat deleted successfully.";
                return RedirectToAction("AllChats");
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("AllChats");
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