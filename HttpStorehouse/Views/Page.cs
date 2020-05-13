using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HttpStorehouse.Models;

namespace HttpStorehouse.Views
{
	public class Page
	{
		private string _css;
		private string _bindable;

		public Page()
		{
			_css = "";
			_bindable = "";
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			using var stream = System.Reflection.Assembly.GetExecutingAssembly()?.GetManifestResourceStream("HttpStorehouse.Views.page.css");
			if (stream != null)
			{
				using var fStreamReader = new StreamReader(stream);
				var strCss = fStreamReader.ReadToEnd();
				_css = strCss;
			}
			
			using var streamBindable = System.Reflection.Assembly.GetExecutingAssembly()?.GetManifestResourceStream("HttpStorehouse.Views.page.bindable");
			if (streamBindable != null)
			{
				using var fStreamReader = new StreamReader(streamBindable);
				var strBindable = fStreamReader.ReadToEnd();
				_bindable = strBindable;
			}
			else
			{
				throw new ArgumentException("Cannot load page");
			}
		}

		public Page BindData<K,V> (List<IModel<K,V>> collection)
		{

			return this;
		}

		public override string ToString()
		{
			return "HAHA printer go brrrrr....";
		}
	}
}
