using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using HttpStorehouse.Models;
using HttpStorehouse.Views;
using NetworkingUtilities.Http.Attributes;
using NetworkingUtilities.Http.Routing;

namespace HttpStorehouse.Controllers
{
	class StoreHouseController : IController
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
					else throw new Exception();
				}
				catch (Exception e)
				{
					Console.WriteLine($"There's no such storehouse{i} in our company");
					Console.WriteLine(e);
				}
			}
		}

		[ControllerRoute("/Company/{id:int}/")]
		private string GetStorehouse(int id)
		{
			var obj = _storeHouses.FirstOrDefault(model => model.Key == id);
			if (obj == null) return null;
			return new Page()
				.BindData(
					obj.Models.Select(product => product as IModel<int, string, string>).ToList(), obj.Description,
					"Company storehouses", obj.Models.Sum(product => long.Parse(product.Value)).ToString()).ToString();
		}

		[ControllerRoute("/Company/{ids:intRange}/")]
		private string GetStorehouses(int[] ids)
		{
			var collection = _storeHouses.Where(store => ids.Contains(store.Key)).ToList();
			var header = string.Join('&', collection.Select(store => store.Description));
			var output = new List<ProductModel>();
			collection.ForEach(store => output.AddRange(store.Models));

			return new Page()
				.BindData<int, string, string>(output.Select(product => product as IModel<int, string, string>).ToList(),
					header, "Company storehouses", output.Sum(product => long.Parse(product.Value)).ToString()).ToString();
		}

	}
}
