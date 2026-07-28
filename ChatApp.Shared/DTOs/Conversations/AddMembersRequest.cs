using System;
using System.Collections.Generic;

namespace ChatApp.Shared.DTOs.Conversations;

public record AddMembersRequest(List<Guid> UserIds);
