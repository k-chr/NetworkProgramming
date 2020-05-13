using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using HttpStorehouse.Models;
using NetworkingUtilities.Http.Attributes;
using NetworkingUtilities.Http.Routing;

namespace HttpStorehouse.Controllers
{
	public class StoreHouseController : IController
	{
		private readonly List<StoreHouseModel> _storeHouses;

		public StoreHouseController()
		{
			_storeHouses = new List<StoreHouseModel>();
			LoadStoreHousesFromJson(5);
		}

		private void LoadStoreHousesFromJson(int howMany)
		{
			for (var i = 1; i <= howMany; ++i)
			{
				try
				{
					using var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream($"HttpStorehouse.Assets.storehouse{i}.json");
					if (stream != null)
					{
						using var fStreamReader = new StreamReader(stream);
						var str = fStreamReader.ReadToEnd();
						var storeHouse = JsonSerializer.Deserialize<StoreHouseModel>(str);
						_storeHouses.Add(storeHouse);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($"There's no such storehouse{i} in our company");
					Console.WriteLine(e);
				}
			}
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
