using Stream.Rest;
using System.Collections.Generic;
using System.Linq;

namespace Stream.Models
{
    public class ActivityMarker
    {
        private bool _allRead = false;
        private bool _allSeen = false;
        private List<string> _read = new List<string>();
        private List<string> _seen = new List<string>();

        private ActivityMarker()
        {
        }

        public ActivityMarker AllRead()
        {
            _allRead = true;
            return this;
        }

        public ActivityMarker AllSeen()
        {
            _allSeen = true;
            return this;
        }

        public ActivityMarker Read(params string[] activityIds)
        {
            if ((!_allRead) && (activityIds != null))
                _read = _read.Union(activityIds).Distinct().ToList();
            return this;
        }

        public ActivityMarker Seen(params string[] activityIds)
        {
            if ((!_allSeen) && (activityIds != null))
                _seen = _seen.Union(activityIds).Distinct().ToList();
            return this;
        }

        internal void Apply(RestRequest request)
        {
            // reads
            if (_allRead)
            {
                request.AddQueryParameter("mark_read", "true");
            }
            else if (_read.Count > 0)
            {
                request.AddQueryParameter("mark_read", string.Join(",", _read));
            }

            // seen
            if (_allSeen)
            {
                request.AddQueryParameter("mark_seen", "true");
            }
            else if (_seen.Count > 0)
            {
                request.AddQueryParameter("mark_seen", string.Join(",", _seen));
            }
        }

        public static ActivityMarker Mark()
        {
            return new ActivityMarker();
        }
    }
}
