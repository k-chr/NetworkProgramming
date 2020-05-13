using System;
using System.Collections.Generic;
using System.Text;
using HttpStorehouse.Models;

namespace HttpStorehouse.Views
{
	class Page
	{
		private string _css;
		private string _bindable;

		public Page()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			

		}

		public Page BindData<K,V> (List<IModel<K,V>> collection)
		{

			return this;
		}
	}
}
