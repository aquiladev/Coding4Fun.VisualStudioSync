using System.Linq;
using System.Collections.Generic;

namespace VisualStudioSync
{
	public class SyncManager : ISyncManager
	{
		private readonly ISyncRepository _repository;
		private readonly IEnumerable<ISyncController> _controllers;

		public SyncManager(ISyncRepository repository, IEnumerable<ISyncController> controllers)
		{
			_repository = repository;
			_controllers = controllers;
		}

		public void Sync()
		{
			var repoValue = _repository.Pull();
			var controllersValue = GetControllersValue();
			if (string.IsNullOrEmpty(repoValue) 
				|| !controllersValue.Equals(repoValue))
			{
				_repository.Push(controllersValue);
			}
		}

		#region Private methods

		private string GetControllersValue()
		{
			return _controllers.Aggregate("", (current, controller) => current + controller.Get());
		}

		#endregion
	}
}
