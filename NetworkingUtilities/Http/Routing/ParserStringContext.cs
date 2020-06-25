using NetworkingUtilities.Extensions;

namespace NetworkingUtilities.Http.Routing
{
	internal class ParserStringContext
	{
		private readonly string _data;
		private int _index;
		private int? _captureStart;

		public ParserStringContext(string data)
		{
			_data = data;
			_index = 0;
			_captureStart = null;
		}

		public char CurrentChar => (char) (_index.InRange(0, _data.Length - 1) ? _data[_index] : 0);

		public string GetMark()
		{
			if (!_captureStart.HasValue) return null;
			var d = _data[_captureStart.Value..(_index + 1)];
			_captureStart = null;
			return d;
		}

		public void Mark() => _captureStart = _index;

		public bool Next() => ++_index < _data.Length;

		public bool Previous() => --_index >= 0;

		public bool HasNext() => _index < _data.Length + 1;
	}
}