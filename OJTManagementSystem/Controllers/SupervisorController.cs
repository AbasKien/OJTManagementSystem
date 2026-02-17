using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
        // HELPER METHODS  ← FIX 1: Only ONE copy of these two methods
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

                // ============================================================
                // ✅ FIXED: CHECK FOR NEW MESSAGES EVERY TIME
                // No more "once per session" blocking!
                // ============================================================
                try
                {
                    var user = await _userManager.GetUserAsync(User);

                    // Get last seen timestamp
                    var lastSeenStr = HttpContext.Session.GetString($"LastSeen_{user.Id}");
                    var lastSeenDate = lastSeenStr != null
                        ? DateTime.Parse(lastSeenStr)
                        : DateTime.MinValue;

                    // Count NEW private messages
                    var conversations = await _chatService.GetUserConversationsAsync(user.Id);
                    int newPrivateMessages = 0;

                    foreach (var conv in conversations)
                    {
                        var messages = await _chatService.GetMessagesByConversationIdAsync(conv.Id);
                        newPrivateMessages += messages.Count(m =>
                            m.SenderId != user.Id &&
                            m.SentAt > lastSeenDate);
                    }

                    // Count NEW group messages
                    var groupChats = await _groupChatService.GetUserGroupChatsAsync(user.Id);
                    int newGroupMessages = 0;

                    foreach (var group in groupChats)
                    {
                        var groupMessages = await _groupChatService.GetGroupChatMessagesAsync(group.GroupChatId);
                        newGroupMessages += groupMessages.Count(m =>
                            m.SenderId != user.Id &&
                            m.CreatedAt > lastSeenDate);
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
                            linkUrl: "/Supervisor/AllChats",
                            linkText: "View Messages"
                        );
                    }
                }
                catch
                {
                    // Silently skip if chat check fails — don't break Dashboard
                }

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

        [HttpGet]
        public async Task<IActionResult> AllChats()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

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
                            IsRead = m.IsRead,
ReadAt = m.ReadAt
                        }).ToList()
                    });
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
                                IsRead = msg.IsRead,      // ✅ FIXED: Changed from 'm' to 'msg'
                                ReadAt = msg.ReadAt       // ✅ FIXED: Changed from 'm' to 'msg'
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

                // ✅ ADD THIS LINE FOR SIGNALR
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
        public IActionResult CreateGroupChat()
        {
            return View(new CreateGroupChatViewModel());
        }

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

                var myInterns = await _internService.GetSupervisorInternsAsync(supervisor.SupervisorId);

                if (!myInterns.Any())
                {
                    TempData["Error"] = "You don't have any assigned interns yet.";
                    return RedirectToAction("ManageInterns");
                }

                var dto = new CreateGroupChatDto
                {
                    GroupName = $"{user.FullName}'s Team",
                    Description = "Group chat for all assigned interns and supervisor"
                };

                var groupChat = await _groupChatService.CreateGroupChatAsync(user.Id, dto);

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

                try
                {
                    var supervisor = await GetCurrentSupervisorAsync();

                    if (supervisor != null)
                    {
                        var myInterns = await _internService.GetSupervisorInternsAsync(supervisor.SupervisorId);
                        var currentMemberIds = groupChat.Members.Select(m => m.UserId).ToList();
                        var availableInterns = myInterns
                            .Where(i => !currentMemberIds.Contains(i.UserId))
                            .ToList();

                        ViewBag.AvailableUsers = availableInterns.Select(i => new
                        {
                            UserId = i.UserId,
                            FullName = $"{i.FirstName} {i.LastName}",
                            Email = i.Email
                        }).ToList();
                    }
                    else
                    {
                        ViewBag.AvailableUsers = new List<object>();
                    }
                }
                catch
                {
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
                // ✅ FIX: Use model.Status == DtrStatus.Approved instead of model.IsApproved
                if (model.Status == DtrStatus.Approved)
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

                    NotificationHelper.ShowError(this, $"Validation Error: {errors}");
                    return View("ApproveDtr", model);
                }

                var dtr = await _dtrService.GetDtrByIdAsync(model.DtrId);
                var dateStr = dtr.RecordDate.ToString("MMM dd, yyyy");

                var dto = MappingHelper.MapApproveDtrViewModelToDto(model);
                await _dtrService.ApproveDtrAsync(dto);

                if (model.Status == DtrStatus.Approved)
                    NotificationHelper.ShowSuccess(this, $"DTR for {dateStr} approved successfully!");
                else
                    NotificationHelper.ShowWarning(this, $"DTR for {dateStr} has been rejected.");

                return RedirectToAction("ViewPendingDtrs");
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowError(this, GetFullErrorMessage(ex));
                return View("ApproveDtr", model);
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
        public async Task<IActionResult> ApproveLeave(ApproveLeaveRequestViewModel model)
        {
            try
            {
                if (model.Status == LeaveStatus.Approved)
                {
                    // Approving: only ApprovedBy is required, clear RejectionReason
                    ModelState.Remove("RejectionReason");
                    model.RejectionReason = string.Empty;
                }
                else
                {
                    // Rejecting: only RejectionReason is required, clear ApprovedBy
                    ModelState.Remove("ApprovedBy");
                    model.ApprovedBy = string.Empty;

                    if (string.IsNullOrWhiteSpace(model.RejectionReason))
                    {
                        ModelState.AddModelError("RejectionReason", "Rejection reason is required when rejecting a leave request.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    NotificationHelper.ShowError(this, $"Validation Error: {errors}");
                    return View("ApproveLeaveRequest", model);
                }

                var leave = await _leaveRequestService.GetLeaveRequestByIdAsync(model.LeaveRequestId);
                var dateRange = $"{leave.StartDate:MMM dd} - {leave.EndDate:MMM dd, yyyy}";

                var dto = MappingHelper.MapApproveLeaveRequestViewModelToDto(model);
                await _leaveRequestService.ApproveLeaveRequestAsync(dto);

                if (model.Status == LeaveStatus.Approved)
                    NotificationHelper.ShowSuccess(this, $"Leave request for {dateRange} approved!");
                else
                    NotificationHelper.ShowWarning(this, $"Leave request for {dateRange} has been rejected.");

                return RedirectToAction("ViewPendingLeaves");
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowError(this, GetFullErrorMessage(ex));
                return View("ApproveLeaveRequest", model);
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

                // FIX 2: Use EvaluateInternViewModel consistently (matches the GET view)
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
        public async Task<IActionResult> SubmitEvaluation(EvaluateInternViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    NotificationHelper.ShowError(this, "Please complete all evaluation criteria.");
                    return View("EvaluateIntern", model);
                }

                // ✅ FIX: Fetch supervisor so SupervisorId is included in the DTO
                // Without this the FK_Evaluations_Supervisors constraint fails
                var supervisor = await GetCurrentSupervisorAsync();
                if (supervisor == null)
                {
                    NotificationHelper.ShowError(this, "Supervisor profile not found. Cannot submit evaluation.");
                    return View("EvaluateIntern", model);
                }

                var dto = new SubmitEvaluationDto
                {
                    InternId = model.InternId,
                    TechnicalCompetence = model.TechnicalCompetence,
                    Punctuality = model.Punctuality,
                    Cooperation = model.Cooperation,
                    Communication = model.Communication,
                    QualityOfWork = model.QualityOfWork,
                    Reliability = model.Reliability,
                    Comments = model.Comments
                };

                // ✅ SupervisorId is passed as first argument, NOT inside the DTO
                // This matches EvaluationService.SubmitEvaluationAsync(int supervisorId, SubmitEvaluationDto dto)
                await _evaluationService.SubmitEvaluationAsync(supervisor.SupervisorId, dto);

                var intern = await _internService.GetInternByIdAsync(model.InternId);
                var internUser = await _userManager.FindByIdAsync(intern.UserId);

                NotificationHelper.NotifyEvaluationSubmitted(this, internUser.FullName);

                return RedirectToAction("ManageInterns");
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowError(this, GetFullErrorMessage(ex));
                return View("EvaluateIntern", model);
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

                await _certificateService.GenerateCertificateAsync(internId, issuedBy);

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
                TempData["Success"] = "Group chat deleted successfully.";
                return RedirectToAction("AllChats");
            }
            catch (Exception ex)
            {
                TempData["Error"] = GetFullErrorMessage(ex);
                return RedirectToAction("AllChats");
            }
        }
    }
}