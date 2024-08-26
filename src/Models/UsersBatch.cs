using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stream.Utils;
using System;
using System.Collections.Generic;

namespace Stream.Models
{
    public class AddUserBatchResponse
    {
        public IEnumerable<User> CreatedUsers { get; set; }
    }

    public class GetUserBatchResponse
    {
        public IEnumerable<User> Users { get; set; }
    }

    public class DeleteUsersBatchResponse
    {
        public IEnumerable<string> DeletedUserIds { get; set; }
    }
}