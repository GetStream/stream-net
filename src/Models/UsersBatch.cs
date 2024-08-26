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

/* Unmerged change from project 'stream-net(netstandard2.0)'
Before:
//    type UsersDeleteBatchResponse struct {
//    	BaseResponse
//    	DeletedUserIDs []string `json:"deleted_user_ids"`
//    }
After:
// type UsersDeleteBatchResponse struct {
//      BaseResponse
//      DeletedUserIDs []string `json:"deleted_user_ids"`
//    }
*/

/* Unmerged change from project 'stream-net(netstandard2.1)'
Before:
//    type UsersDeleteBatchResponse struct {
//    	BaseResponse
//    	DeletedUserIDs []string `json:"deleted_user_ids"`
//    }
After:
// type UsersDeleteBatchResponse struct {
//      BaseResponse
//      DeletedUserIDs []string `json:"deleted_user_ids"`
//    }
*/

/* Unmerged change from project 'stream-net(net8.0)'
Before:
//    type UsersDeleteBatchResponse struct {
//    	BaseResponse
//    	DeletedUserIDs []string `json:"deleted_user_ids"`
//    }
After:
// type UsersDeleteBatchResponse struct {
//      BaseResponse
//      DeletedUserIDs []string `json:"deleted_user_ids"`
//    }
*/
    //    type UsersDeleteBatchResponse struct {
    //    	BaseResponse
    //    	DeletedUserIDs []string `json:"deleted_user_ids"`
    //    }

    public class DeleteUsersBatchResponse
    {
        public IEnumerable<string> DeletedUserIds { get; set; }
    }
}




// type UsersGetResponse struct {
// Users []*types.User `json:"data"`
// }