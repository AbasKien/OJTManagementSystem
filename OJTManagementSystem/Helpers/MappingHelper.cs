using OJTManagementSystem.Dtos;
using OJTManagementSystem.Enums;
using OJTManagementSystem.Models;
using OJTManagementSystem.ViewModel;

namespace OJTManagementSystem.Helpers
{
    public static class MappingHelper
    {
        // ========================================
        // DTR MAPPINGS
        // ========================================

        /// <summary>
        /// Map SubmitDtrViewModel to SubmitDtrDto
        /// </summary>
        public static SubmitDtrDto MapDtrViewModelToDto(SubmitDtrViewModel viewModel)
        {
            if (viewModel == null)
                return null;

            return new SubmitDtrDto
            {
                RecordDate = viewModel.RecordDate,
                TimeIn = viewModel.TimeIn,
                TimeOut = viewModel.TimeOut,
                ActivityDescription = viewModel.ActivityDescription
            };
        }

        /// <summary>
        /// Map SubmitDtrDto to SubmitDtrViewModel
        /// </summary>
        public static SubmitDtrViewModel MapDtrDtoToViewModel(SubmitDtrDto dto)
        {
            if (dto == null)
                return null;

            return new SubmitDtrViewModel
            {
                RecordDate = dto.RecordDate,
                TimeIn = dto.TimeIn,
                TimeOut = dto.TimeOut,
                ActivityDescription = dto.ActivityDescription
            };
        }

        /// <summary>
        /// Map DailyTimeRecord entity to DailyTimeRecordViewModel
        /// </summary>
        public static DailyTimeRecordViewModel MapDtrToViewModel(DailyTimeRecord dtr)
        {
            if (dtr == null)
                return null;

            return new DailyTimeRecordViewModel
            {
                DtrId = dtr.DtrId,
                InternId = dtr.InternId,
                InternName = dtr.Intern?.User?.FullName,
                RecordDate = dtr.RecordDate,
                TimeIn = dtr.TimeIn,
                TimeOut = dtr.TimeOut,
                ActivityDescription = dtr.ActivityDescription,
                TotalHours = dtr.TotalHours,
                Status = dtr.Status,
                RejectionReason = dtr.RejectionReason,
                CreatedAt = dtr.CreatedAt,
                UpdatedAt = dtr.UpdatedAt,
                ApprovedAt = dtr.ApprovedAt
            };
        }

        /// <summary>
        /// Map DailyTimeRecordViewModel to ApproveDtrViewModel for approval form
        /// </summary>
        public static ApproveDtrViewModel MapDtrViewModelToApprovalViewModel(DailyTimeRecordViewModel dtr)
        {
            if (dtr == null)
                return null;

            return new ApproveDtrViewModel
            {
                DtrId = dtr.DtrId,
                InternId = dtr.InternId,
                InternName = dtr.InternName,
                RecordDate = dtr.RecordDate,
                TimeIn = dtr.TimeIn,
                TimeOut = dtr.TimeOut,
                ActivityDescription = dtr.ActivityDescription,
                TotalHours = dtr.TotalHours,
                CurrentStatus = dtr.Status,
                CreatedAt = dtr.CreatedAt,
                Status = DtrStatus.Approved, // Default selection
                RejectionReason = string.Empty
            };
        }

        /// <summary>
        /// Map DailyTimeRecord entity to ApproveDtrViewModel for approval form
        /// </summary>
        public static ApproveDtrViewModel MapDtrToApprovalViewModel(DailyTimeRecord dtr)
        {
            if (dtr == null)
                return null;

            return new ApproveDtrViewModel
            {
                DtrId = dtr.DtrId,
                InternId = dtr.InternId,
                InternName = dtr.Intern?.User?.FullName,
                RecordDate = dtr.RecordDate,
                TimeIn = dtr.TimeIn,
                TimeOut = dtr.TimeOut,
                ActivityDescription = dtr.ActivityDescription,
                TotalHours = dtr.TotalHours,
                CurrentStatus = dtr.Status,
                CreatedAt = dtr.CreatedAt,
                Status = DtrStatus.Approved, // Default selection
                RejectionReason = string.Empty
            };
        }

        /// <summary>
        /// Map ApproveDtrViewModel to ApproveDtrDto
        /// </summary>
        public static ApproveDtrDto MapApproveDtrViewModelToDto(ApproveDtrViewModel viewModel)
        {
            if (viewModel == null)
                return null;

            return new ApproveDtrDto
            {
                DtrId = viewModel.DtrId,
                Status = viewModel.Status,
                RejectionReason = viewModel.RejectionReason ?? string.Empty
            };
        }

        // ========================================
        // LEAVE REQUEST MAPPINGS
        // ========================================

        /// <summary>
        /// Map SubmitLeaveRequestViewModel to SubmitLeaveRequestDto
        /// </summary>
        public static SubmitLeaveRequestDto MapLeaveRequestViewModelToDto(SubmitLeaveRequestViewModel viewModel)
        {
            if (viewModel == null)
                return null;

            return new SubmitLeaveRequestDto
            {
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                LeaveType = viewModel.LeaveType,
                Reason = viewModel.Reason
            };
        }

        /// <summary>
        /// Map SubmitLeaveRequestDto to SubmitLeaveRequestViewModel
        /// </summary>
        public static SubmitLeaveRequestViewModel MapLeaveRequestDtoToViewModel(SubmitLeaveRequestDto dto)
        {
            if (dto == null)
                return null;

            return new SubmitLeaveRequestViewModel
            {
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                LeaveType = dto.LeaveType,
                Reason = dto.Reason
            };
        }

        /// <summary>
        /// Map LeaveRequest entity to LeaveRequestViewModel
        /// </summary>
        public static LeaveRequestViewModel MapLeaveRequestToViewModel(LeaveRequest leaveRequest)
        {
            if (leaveRequest == null)
                return null;

            return new LeaveRequestViewModel
            {
                LeaveRequestId = leaveRequest.LeaveRequestId,
                InternId = leaveRequest.InternId,
                InternName = leaveRequest.Intern?.User?.FullName,
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                LeaveType = leaveRequest.LeaveType,
                Reason = leaveRequest.Reason,
                Status = leaveRequest.Status,
                RejectionReason = leaveRequest.RejectionReason,
                NumberOfDays = leaveRequest.NumberOfDays,
                CreatedAt = leaveRequest.CreatedAt,
                UpdatedAt = leaveRequest.UpdatedAt,
                ApprovedAt = leaveRequest.ApprovedAt
            };
        }

        /// <summary>
        /// Map LeaveRequestViewModel to ApproveLeaveRequestViewModel for approval form
        /// </summary>
        public static ApproveLeaveRequestViewModel MapLeaveRequestViewModelToApprovalViewModel(LeaveRequestViewModel leave)
        {
            if (leave == null)
                return null;

            return new ApproveLeaveRequestViewModel
            {
                LeaveRequestId = leave.LeaveRequestId,
                InternId = leave.InternId,
                InternName = leave.InternName,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                LeaveType = leave.LeaveType,
                Reason = leave.Reason,
                NumberOfDays = leave.NumberOfDays,
                CurrentStatus = leave.Status,
                CreatedAt = leave.CreatedAt,
                Status = LeaveStatus.Approved, // Default selection
                ApprovedBy = string.Empty,
                RejectionReason = string.Empty
            };
        }

        /// <summary>
        /// Map LeaveRequest entity to ApproveLeaveRequestViewModel for approval form
        /// </summary>
        public static ApproveLeaveRequestViewModel MapLeaveRequestToApprovalViewModel(LeaveRequest leaveRequest)
        {
            if (leaveRequest == null)
                return null;

            return new ApproveLeaveRequestViewModel
            {
                LeaveRequestId = leaveRequest.LeaveRequestId,
                InternId = leaveRequest.InternId,
                InternName = leaveRequest.Intern?.User?.FullName,
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                LeaveType = leaveRequest.LeaveType,
                Reason = leaveRequest.Reason,
                NumberOfDays = leaveRequest.NumberOfDays,
                CurrentStatus = leaveRequest.Status,
                CreatedAt = leaveRequest.CreatedAt,
                Status = LeaveStatus.Approved, // Default selection
                ApprovedBy = string.Empty,
                RejectionReason = string.Empty
            };
        }

        /// <summary>
        /// Map ApproveLeaveRequestViewModel to ApproveLeaveRequestDto
        /// </summary>
        public static ApproveLeaveRequestDto MapApproveLeaveRequestViewModelToDto(ApproveLeaveRequestViewModel viewModel)
        {
            if (viewModel == null)
                return null;

            return new ApproveLeaveRequestDto
            {
                LeaveRequestId = viewModel.LeaveRequestId,
                Status = viewModel.Status,
                ApprovedBy = viewModel.ApprovedBy ?? string.Empty,
                RejectionReason = viewModel.RejectionReason ?? string.Empty
            };
        }

        // ========================================
        // INTERN MAPPINGS
        // ========================================

        /// <summary>
        /// Map Intern entity to InternViewModel
        /// </summary>
        public static InternViewModel MapInternToViewModel(Intern intern)
        {
            if (intern == null)
                return null;

            return new InternViewModel
            {
                InternId = intern.InternId,
                UserId = intern.UserId,
                FirstName = intern.User?.FirstName,
                LastName = intern.User?.LastName,
                Email = intern.User?.Email,
                PhoneNumber = intern.User?.PhoneNumber,
                StudentId = intern.StudentId,
                School = intern.School,
                Course = intern.Course,
                Department = intern.Department,
                StartDate = intern.StartDate,
                EndDate = intern.EndDate,
                SupervisorId = intern.SupervisorId,
                SupervisorName = intern.Supervisor?.User?.FullName ?? "Unassigned",
                SupervisorUserId = intern.Supervisor?.UserId,
                IsActive = intern.IsActive,
                CreatedAt = intern.CreatedAt,
                UpdatedAt = intern.UpdatedAt
            };
        }

        /// <summary>
        /// Map RegisterInternDto to Intern entity (for registration)
        /// </summary>
        public static Intern MapRegisterInternDtoToIntern(RegisterInternDto dto, string userId)
        {
            if (dto == null)
                return null;

            return new Intern
            {
                UserId = userId,
                StudentId = dto.StudentId,
                School = dto.School,
                Course = dto.Course,
                Department = dto.Department,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                SupervisorId = dto.SupervisorId > 0 ? dto.SupervisorId : null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Map RegisterViewModel to RegisterInternDto
        /// </summary>
        public static RegisterInternDto MapRegisterViewModelToDto(RegisterViewModel viewModel)
        {
            if (viewModel == null || viewModel.Role != "Intern")
                return null;

            return new RegisterInternDto
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Email = viewModel.Email,
                PhoneNumber = viewModel.PhoneNumber,
                Password = viewModel.Password,
                ConfirmPassword = viewModel.ConfirmPassword,
                StudentId = viewModel.StudentId,
                School = viewModel.School,
                Course = viewModel.Course,
                Department = viewModel.Department,
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                SupervisorId = 0 // Default, can be assigned later
            };
        }

        // ========================================
        // EVALUATION MAPPINGS
        // ========================================

        /// <summary>
        /// Map Evaluation entity to EvaluationViewModel
        /// </summary>
        public static EvaluationViewModel MapEvaluationToViewModel(Evaluation evaluation)
        {
            if (evaluation == null)
                return null;

            return new EvaluationViewModel
            {
                EvaluationId = evaluation.EvaluationId,
                InternId = evaluation.InternId,
                InternName = evaluation.Intern?.User?.FullName,
                SupervisorId = evaluation.SupervisorId,
                SupervisorName = evaluation.Supervisor?.User?.FullName,
                TechnicalCompetence = evaluation.TechnicalCompetence,
                Punctuality = evaluation.Punctuality,
                Cooperation = evaluation.Cooperation,
                Communication = evaluation.Communication,
                QualityOfWork = evaluation.QualityOfWork,
                Reliability = evaluation.Reliability,
                FinalRating = evaluation.FinalRating,
                Comments = evaluation.Comments,
                CreatedAt = evaluation.CreatedAt,
                UpdatedAt = evaluation.UpdatedAt
            };
        }

        /// <summary>
        /// Map EvaluateInternViewModel to SubmitEvaluationDto
        /// </summary>
        public static SubmitEvaluationDto MapEvaluateInternViewModelToDto(EvaluateInternViewModel viewModel)
        {
            if (viewModel == null)
                return null;

            return new SubmitEvaluationDto
            {
                InternId = viewModel.InternId,
                TechnicalCompetence = viewModel.TechnicalCompetence,
                Punctuality = viewModel.Punctuality,
                Cooperation = viewModel.Cooperation,
                Communication = viewModel.Communication,
                QualityOfWork = viewModel.QualityOfWork,
                Reliability = viewModel.Reliability,
                Comments = viewModel.Comments
            };
        }

        /// <summary>
        /// Map SubmitEvaluationDto to EvaluateInternViewModel
        /// </summary>
        public static EvaluateInternViewModel MapSubmitEvaluationDtoToViewModel(SubmitEvaluationDto dto)
        {
            if (dto == null)
                return null;

            return new EvaluateInternViewModel
            {
                InternId = dto.InternId,
                TechnicalCompetence = dto.TechnicalCompetence,
                Punctuality = dto.Punctuality,
                Cooperation = dto.Cooperation,
                Communication = dto.Communication,
                QualityOfWork = dto.QualityOfWork,
                Reliability = dto.Reliability,
                Comments = dto.Comments
            };
        }

        // ========================================
        // CERTIFICATE MAPPINGS
        // ========================================

        /// <summary>
        /// Map Certificate entity to CertificateViewModel
        /// </summary>
        public static CertificateViewModel MapCertificateToViewModel(Certificate certificate)
        {
            if (certificate == null)
                return null;

            return new CertificateViewModel
            {
                CertificateId = certificate.CertificateId,
                InternId = certificate.InternId,
                InternName = certificate.Intern?.User?.FullName,
                School = certificate.Intern?.School,
                Course = certificate.Intern?.Course,
                CertificateNumber = certificate.CertificateNumber,
                IssuedDate = certificate.IssuedDate,
                TotalHoursRendered = certificate.TotalHoursRendered,
                StartDate = certificate.StartDate,
                EndDate = certificate.EndDate,
                IssuedBy = certificate.IssuedBy,
                PdfFileName = certificate.PdfFileName,
                CreatedAt = certificate.CreatedAt
            };
        }

        // ========================================
        // CHAT MESSAGE MAPPINGS
        // ========================================

        /// <summary>
        /// Map ChatMessage entity to ChatMessageViewModel
        /// </summary>
        public static ChatMessageViewModel MapChatMessageToViewModel(ChatMessage message)
        {
            if (message == null)
                return null;

            // Note: ChatMessage model doesn't have ReceiverId, IsRead, or ReadAt properties
            // We'll need to determine the receiver from the Conversation
            string receiverId = null;
            string receiverName = null;

            if (message.Conversation != null)
            {
                // Determine who the receiver is (the other person in the conversation)
                receiverId = message.Conversation.User1Id == message.SenderId
                    ? message.Conversation.User2Id
                    : message.Conversation.User1Id;

                // You may need to load the receiver's information separately
                // This is a simplified version
            }

            return new ChatMessageViewModel
            {
                ChatMessageId = message.Id,
                SenderId = message.SenderId,
                SenderName = null, // Needs to be populated from ApplicationUser
                ReceiverId = receiverId,
                ReceiverName = receiverName,
                MessageContent = message.Content,
                IsRead = false, // Not available in ChatMessage model
                CreatedAt = message.SentAt,
                ReadAt = null // Not available in ChatMessage model
            };
        }

        /// <summary>
        /// Map SendChatMessageDto to ChatMessage entity
        /// </summary>
        public static ChatMessage MapSendChatMessageDtoToChatMessage(SendChatMessageDto dto, int conversationId, string senderId)
        {
            if (dto == null)
                return null;

            return new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = dto.MessageContent,
                SentAt = DateTime.Now
            };
        }

        /// <summary>
        /// Map ChatMessage to MessageViewModel (for MessageListViewModel)
        /// </summary>
        public static MessageViewModel MapChatMessageToMessageViewModel(ChatMessage message, bool isSentByMe)
        {
            if (message == null)
                return null;

            string receiverId = null;
            string receiverName = null;

            if (message.Conversation != null)
            {
                receiverId = message.Conversation.User1Id == message.SenderId
                    ? message.Conversation.User2Id
                    : message.Conversation.User1Id;
            }

            return new MessageViewModel
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = null, // Needs to be populated
                ReceiverId = receiverId,
                ReceiverName = receiverName,
                Subject = null, // Not available in ChatMessage
                Content = message.Content,
                DateSent = message.SentAt,
                IsRead = false, // Not available in ChatMessage
                IsSentByMe = isSentByMe,
                ReadAt = null // Not available in ChatMessage
            };
        }

       
        // ========================================
        // GROUP CHAT MAPPINGS
        // ========================================

        /// <summary>
        /// Map GroupChat entity to GroupChatViewModel
        /// </summary>
        public static GroupChatViewModel MapGroupChatToViewModel(GroupChat groupChat)
        {
            if (groupChat == null)
                return null;

            return new GroupChatViewModel
            {
                GroupChatId = groupChat.GroupChatId,
                GroupName = groupChat.GroupName,
                Description = groupChat.Description,
                CreatorId = groupChat.CreatedBy,
                CreatorName = groupChat.Creator?.FullName,
                CreatedAt = groupChat.CreatedAt,
                MemberCount = groupChat.Members?.Count ?? 0,
                MessageCount = groupChat.Messages?.Count ?? 0,
                Members = groupChat.Members?.Select(MapGroupChatMemberToViewModel).ToList() ?? new List<GroupChatMemberViewModel>(),
                Messages = groupChat.Messages?.Select(MapGroupChatMessageToViewModel).ToList() ?? new List<GroupChatMessageViewModel>()
            };
        }

        /// <summary>
        /// Map GroupChatMember entity to GroupChatMemberViewModel
        /// </summary>
        public static GroupChatMemberViewModel MapGroupChatMemberToViewModel(GroupChatMember member)
        {
            if (member == null)
                return null;

            return new GroupChatMemberViewModel
            {
                GroupChatMemberId = member.GroupChatMemberId,
                UserId = member.UserId,
                UserName = member.User?.FullName,
                IsAdmin = member.IsAdmin,
                JoinedAt = member.JoinedAt
            };
        }

        /// <summary>
        /// Map GroupChatMessage entity to GroupChatMessageViewModel
        /// </summary>
        public static GroupChatMessageViewModel MapGroupChatMessageToViewModel(GroupChatMessage message)
        {
            if (message == null)
                return null;

            return new GroupChatMessageViewModel
            {
                GroupChatMessageId = message.GroupChatMessageId,
                GroupChatId = message.GroupChatId,
                SenderId = message.SenderId,
                SenderName = message.Sender?.FullName,
                MessageContent = message.MessageContent,
                CreatedAt = message.CreatedAt
            };
        }

        /// <summary>
        /// Map CreateGroupChatViewModel to CreateGroupChatDto
        /// </summary>
        public static CreateGroupChatDto MapCreateGroupChatViewModelToDto(CreateGroupChatViewModel viewModel)
        {
            if (viewModel == null)
                return null;

            return new CreateGroupChatDto
            {
                GroupName = viewModel.GroupName,
                Description = viewModel.Description
            };
        }

        /// <summary>
        /// Map CreateGroupChatDto to CreateGroupChatViewModel
        /// </summary>
        public static CreateGroupChatViewModel MapCreateGroupChatDtoToViewModel(CreateGroupChatDto dto)
        {
            if (dto == null)
                return null;

            return new CreateGroupChatViewModel
            {
                GroupName = dto.GroupName,
                Description = dto.Description
            };
        }

        /// <summary>
        /// Map CreateGroupChatDto to GroupChat entity
        /// </summary>
        public static GroupChat MapCreateGroupChatDtoToGroupChat(CreateGroupChatDto dto, string createdBy)
        {
            if (dto == null)
                return null;

            return new GroupChat
            {
                GroupName = dto.GroupName,
                Description = dto.Description,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        /// <summary>
        /// Map SendGroupChatMessageDto to GroupChatMessage entity
        /// </summary>
        public static GroupChatMessage MapSendGroupChatMessageDtoToMessage(SendGroupChatMessageDto dto, int groupChatId, string senderId)
        {
            if (dto == null)
                return null;

            return new GroupChatMessage
            {
                GroupChatId = groupChatId,
                SenderId = senderId,
                MessageContent = dto.MessageContent,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Map AddGroupChatMemberDto to GroupChatMember entity
        /// </summary>
        public static GroupChatMember MapAddGroupChatMemberDtoToMember(AddGroupChatMemberDto dto, int groupChatId)
        {
            if (dto == null)
                return null;

            return new GroupChatMember
            {
                GroupChatId = groupChatId,
                UserId = dto.UserId,
                JoinedAt = DateTime.UtcNow,
                IsAdmin = false
            };
        }
        // ========================================
        // CONVERSATION MAPPINGS
        // ========================================

        /// <summary>
        /// Map Conversation to PrivateChatViewModel
        /// </summary>
        public static PrivateChatViewModel MapConversationToPrivateChatViewModel(Conversation conversation, string currentUserId)
        {
            if (conversation == null)
                return null;

            // Determine the other user in the conversation
            string otherUserId = conversation.User1Id == currentUserId ? conversation.User2Id : conversation.User1Id;

            return new PrivateChatViewModel
            {
                OtherUserId = otherUserId,
                OtherUserName = null, // Needs to be populated separately
                OtherUserRole = null, // Needs to be populated separately
                Messages = conversation.Messages?.Select(m => MapChatMessageToViewModel(m)).ToList() ?? new List<ChatMessageViewModel>()
            };
        }

        // ========================================
        // DASHBOARD MAPPINGS
        // ========================================

        /// <summary>
        /// Create InternDashboardViewModel from various data sources
        /// </summary>
        public static InternDashboardViewModel CreateInternDashboardViewModel(
            InternViewModel internProfile,
            List<DailyTimeRecordViewModel> allDtrs,
            List<LeaveRequestViewModel> allLeaves,
            EvaluationViewModel latestEvaluation,
            CertificateViewModel certificate)
        {
            var recentDtrs = allDtrs?.OrderByDescending(d => d.RecordDate).Take(5).ToList() ?? new List<DailyTimeRecordViewModel>();

            return new InternDashboardViewModel
            {
                InternProfile = internProfile,
                TotalDtrSubmitted = allDtrs?.Count ?? 0,
                ApprovedDtrCount = allDtrs?.Count(d => d.Status == DtrStatus.Approved) ?? 0,
                PendingDtrCount = allDtrs?.Count(d => d.Status == DtrStatus.Pending) ?? 0,
                RejectedDtrCount = allDtrs?.Count(d => d.Status == DtrStatus.Rejected) ?? 0,
                TotalHoursRendered = allDtrs?.Where(d => d.Status == DtrStatus.Approved).Sum(d => d.TotalHours) ?? 0,
                ApprovedLeaveCount = allLeaves?.Count(l => l.Status == LeaveStatus.Approved) ?? 0,
                PendingLeaveCount = allLeaves?.Count(l => l.Status == LeaveStatus.Pending) ?? 0,
                LatestEvaluation = latestEvaluation,
                Certificate = certificate,
                RecentDtrs = recentDtrs
            };
        }

        /// <summary>
        /// Create SupervisorDashboardViewModel from various data sources
        /// </summary>
        public static SupervisorDashboardViewModel CreateSupervisorDashboardViewModel(
            List<InternViewModel> interns,
            List<DailyTimeRecordViewModel> pendingDtrs,
            List<LeaveRequestViewModel> pendingLeaves,
            int pendingEvaluationCount,
            int unreadMessageCount = 0,
            List<GroupChatViewModel> recentGroupChats = null,
            List<PrivateChatViewModel> recentPrivateChats = null)
        {
            return new SupervisorDashboardViewModel
            {
                InternCount = interns?.Count ?? 0,
                PendingDtrCount = pendingDtrs?.Count ?? 0,
                PendingLeaveRequestCount = pendingLeaves?.Count ?? 0,
                PendingEvaluationCount = pendingEvaluationCount,
                PendingDtrs = pendingDtrs ?? new List<DailyTimeRecordViewModel>(),
                PendingLeaveRequests = pendingLeaves ?? new List<LeaveRequestViewModel>(),
                Interns = interns ?? new List<InternViewModel>(),
                UnreadMessageCount = unreadMessageCount,
                RecentGroupChats = recentGroupChats ?? new List<GroupChatViewModel>(),
                RecentPrivateChats = recentPrivateChats ?? new List<PrivateChatViewModel>()
            };
        }

        // ========================================
        // APPLICATION USER MAPPINGS
        // ========================================

        /// <summary>
        /// Map RegisterViewModel to ApplicationUser
        /// </summary>
        public static ApplicationUser MapRegisterViewModelToApplicationUser(RegisterViewModel viewModel)
        {
            if (viewModel == null)
                return null;

            return new ApplicationUser
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Email = viewModel.Email,
                UserName = viewModel.Email,
                PhoneNumber = viewModel.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Map RegisterViewModel to Supervisor entity
        /// </summary>
        public static Supervisor MapRegisterViewModelToSupervisor(RegisterViewModel viewModel, string userId)
        {
            if (viewModel == null || viewModel.Role != "Supervisor")
                return null;

            return new Supervisor
            {
                UserId = userId,
                Position = viewModel.Position ?? "Supervisor",
                Department = viewModel.Department,
                PhoneNumber = viewModel.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        // ========================================
        // HELPER METHODS FOR LIST CONVERSIONS
        // ========================================

        /// <summary>
        /// Map list of DailyTimeRecord to list of DailyTimeRecordViewModel
        /// </summary>
        public static List<DailyTimeRecordViewModel> MapDtrListToViewModelList(IEnumerable<DailyTimeRecord> dtrs)
        {
            if (dtrs == null)
                return new List<DailyTimeRecordViewModel>();

            return dtrs.Select(MapDtrToViewModel).ToList();
        }

        /// <summary>
        /// Map list of LeaveRequest to list of LeaveRequestViewModel
        /// </summary>
        public static List<LeaveRequestViewModel> MapLeaveRequestListToViewModelList(IEnumerable<LeaveRequest> leaves)
        {
            if (leaves == null)
                return new List<LeaveRequestViewModel>();

            return leaves.Select(MapLeaveRequestToViewModel).ToList();
        }

        /// <summary>
        /// Map list of Intern to list of InternViewModel
        /// </summary>
        public static List<InternViewModel> MapInternListToViewModelList(IEnumerable<Intern> interns)
        {
            if (interns == null)
                return new List<InternViewModel>();

            return interns.Select(MapInternToViewModel).ToList();
        }

        /// <summary>
        /// Map list of Evaluation to list of EvaluationViewModel
        /// </summary>
        public static List<EvaluationViewModel> MapEvaluationListToViewModelList(IEnumerable<Evaluation> evaluations)
        {
            if (evaluations == null)
                return new List<EvaluationViewModel>();

            return evaluations.Select(MapEvaluationToViewModel).ToList();
        }

        /// <summary>
        /// Map list of Certificate to list of CertificateViewModel
        /// </summary>
        public static List<CertificateViewModel> MapCertificateListToViewModelList(IEnumerable<Certificate> certificates)
        {
            if (certificates == null)
                return new List<CertificateViewModel>();

            return certificates.Select(MapCertificateToViewModel).ToList();
        }

        /// <summary>
        /// Map list of GroupChat to list of GroupChatViewModel
        /// </summary>
        public static List<GroupChatViewModel> MapGroupChatListToViewModelList(IEnumerable<GroupChat> groupChats)
        {
            if (groupChats == null)
                return new List<GroupChatViewModel>();

            return groupChats.Select(MapGroupChatToViewModel).ToList();
        }
    }
}