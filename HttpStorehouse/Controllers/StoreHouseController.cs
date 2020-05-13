using System;
using System.Collections.Generic;
using System.Text;
using HttpStorehouse.Models;
using NetworkingUtilities.Http.Attributes;
using NetworkingUtilities.Http.Routing;

namespace HttpStorehouse.Controllers
{
	public class StoreHouseController : IController
	{
		private List<StoreHouseModel> _storeHouses;

		public void LoadStoreHousesFromJson()
		{

		}

		[ControllerRoute("/Company/{id:int}/")]
		public string GetStorehouse(int id)
		{
			return "";
		}

		[ControllerRoute("/Company/{ids:intRange}/")]
		public string GetStorehouses(int[] ids)
		{
			return "";
		}

	}
}
