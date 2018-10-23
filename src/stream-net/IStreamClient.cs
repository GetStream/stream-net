using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stream
{
	public interface IStreamClient
	{
		IBatchOperations Batch { get; }
		Collections Collections { get; }

		Task ActivityPartialUpdate(string id = null, ForeignIDTime foreignIDTime = null, GenericData set = null, IEnumerable<string> unset = null);
		IStreamFeed Feed(string feedSlug, string userId);
	}
}