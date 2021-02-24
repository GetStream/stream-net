using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
    public interface IBatchOperations
    {
        Task AddToMany(Activity activity, IEnumerable<IStreamFeed> feeds);
        Task AddToMany(Activity activity, IEnumerable<string> feedIds);
        Task FollowMany(IEnumerable<Follow> follows, int activityCopyLimit = 300);
        Task<IEnumerable<Activity>> GetActivities(IEnumerable<string> ids = null, IEnumerable<ForeignIDTime> foreignIDTimes = null);
        Task UpdateActivities(IEnumerable<Activity> activities);
        Task UpdateActivity(Activity activity);
        Task ActivitiesPartialUpdate(IEnumerable<ActivityPartialUpdateRequestObject> updates);
    }
}
