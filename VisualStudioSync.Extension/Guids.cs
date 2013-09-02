// Guids.cs
// MUST match guids.h
using System;

namespace VisualStudioSync.Extension
{
	static class GuidList
	{
		public const string guidVisualStudioSyncPkgString = "11eb8bce-131f-4c89-8afe-976a5c4670cf";
		public const string guidVisualStudioSyncCmdSetString = "f0892707-7544-4808-850d-eb3285b7cf39";

		public static readonly Guid guidVisualStudioSyncCmdSet = new Guid(guidVisualStudioSyncCmdSetString);
	};
}