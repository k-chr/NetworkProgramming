using System;
using System.Linq;
using HttpStorehouse.Views;
using NetworkingUtilities.Http.Attributes;
using NetworkingUtilities.Http.Routing;

namespace HttpStorehouse.Controllers
{
	class SampleController : IController
	{
		[ControllerRoute(url: "/Sample/String/{id:int}/{name:alpha}/{values:intRange}/", verb: "GET")]
		string GetString(int id, string name, int[] values)
		{
			Console.WriteLine($"{id} | {name} | values: {string.Join(',', values.Select(v => v.ToString()).ToArray())}");
			return new Page().ToString();
		}
	}
}
