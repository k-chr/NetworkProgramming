using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NetworkingUtilities.Extensions;

namespace NetworkingUtilities.Http.Routing
{
	public class RouteParser
	{
		private enum ValidCharacters
		{
			CurlyOpenBrace,
			CurlyClosedBrace,
			EqualsSign,
			OptionalSign,
			SubPathSign,
			ValuesSeparator,
			ConstraintIndicator,
			NormalOpenBrace,
			NormalClosedBrace,
			DefaultsSeparator
		}

		private readonly Dictionary<ValidCharacters, char> _availableCharacters = new Dictionary<ValidCharacters, char>
		{
			{ValidCharacters.CurlyOpenBrace, '{'},
			{ValidCharacters.CurlyClosedBrace, '}'},
			{ValidCharacters.EqualsSign, '='},
			{ValidCharacters.OptionalSign, '?'},
			{ValidCharacters.SubPathSign, '/'},
			{ValidCharacters.ValuesSeparator, '_'},
			{ValidCharacters.ConstraintIndicator, ':'},
			{ValidCharacters.NormalOpenBrace, '('},
			{ValidCharacters.NormalClosedBrace, ')'},
			{ValidCharacters.DefaultsSeparator, ','}
		};

		private readonly Dictionary<string, string> _availableMatchers = new Dictionary<string, string>
		{
			{"int", @"^\d+$"},
			{"double", @"^-?\d+(?:\.\d+)$"},
			{"intRange", @"^-?\d+(?:_-?\d+)*$"},
			{"alpha", @"^[a-zA-Z]+$"}
		};

		private readonly HashSet<string> _availableParametrizedMatchers = new HashSet<string>
		{
			"length", "min", "max", "inRange", "date"
		};

		private ParserStringContext _ctx;
		private int _currentSegment;
		private int _currentQuestionMark;
		private int _currentEqualSignMark;

		public RoutePattern ParsePattern(string uri)
		{
			_ctx = new ParserStringContext(uri);

			if (_ctx.CurrentChar == _availableCharacters[ValidCharacters.SubPathSign])
			{
				++_currentSegment;
			}

			var routeElems = new List<IRouteElement>();

			while (_ctx.HasNext())
			{
				_ctx.Next();

				if (_ctx.CurrentChar == _availableCharacters[ValidCharacters.CurlyOpenBrace])
				{
					var elem = ParsePatternParameter();
					routeElems.Add(elem);
				}
				else if (char.IsLetter(_ctx.CurrentChar))
				{
					var elem = ParsePatternLiteral();
					routeElems.Add(elem);
				}

				if (_ctx.CurrentChar == _availableCharacters[ValidCharacters.SubPathSign])
				{
					++_currentSegment;
				}
			}

			return new RoutePattern(uri, routeElems);
		}

		private RouteLiteral ParsePatternLiteral()
		{
			_ctx.Mark();
			while (_ctx.HasNext() && char.IsLetter(_ctx.CurrentChar))
			{
				_ctx.Next();
			}

			_ctx.Previous();
			var key = _ctx.GetMark();
			_ctx.Next();

			if (_availableCharacters.Values.Any(c => key.Contains(c)) ||
				(_ctx.HasNext() && _ctx.CurrentChar != _availableCharacters[ValidCharacters.SubPathSign]))
				throw new Exception();

			return new RouteLiteral(key, _currentSegment);
		}

		private RouteParam ParsePatternParameter()
		{
			_ctx.Mark();
			_ctx.Next();

			//parser expects literal to match parameter name :)
			if (_availableCharacters.ContainsValue(_ctx.CurrentChar)) return null;

			var constraints = new Dictionary<string, Func<object, bool>>();
			var defaults = new List<string>();

			while (_ctx.HasNext() && char.IsLetter(_ctx.CurrentChar))
			{
				_ctx.Next();
			}

			_ctx.Previous();
			var name = _ctx.GetMark().Replace("{", "").Replace("}", "");
			_ctx.Next();
			if (char.IsNumber(_ctx.CurrentChar) || !_availableCharacters.ContainsValue(_ctx.CurrentChar))
				throw new Exception();

			var optional = false;

			if (_ctx.HasNext() && _ctx.CurrentChar != _availableCharacters[ValidCharacters.SubPathSign])
			{
				object obj = _ctx.CurrentChar switch
							 {
								 ':' => ParseConstraint(),
								 '=' => ParseDefaults(),
								 '?' => SetOptional(),
								 _ => throw new Exception()
							 };

				switch (obj)
				{
					case (string s, Func<object, bool> func):
						constraints.Add(s, func);
						break;
					case bool b:
						optional = b;
						break;
					case List<string> listOfDefaults:
						defaults = listOfDefaults;
						break;
				}
			}

			return new RouteParam(name, _currentSegment, optional, constraints, defaults);
		}

		private bool SetOptional()
		{
			if (_currentQuestionMark == 1) throw new Exception();
			++_currentQuestionMark;
			return true;
		}

		private List<string> ParseDefaults()
		{
			if (_currentEqualSignMark == 1) throw new Exception();

			++_currentEqualSignMark;

			_ctx.Next();

			var defaults = new List<string>();

			do
			{
				var @default = GetDefault();
				defaults.Add(@default);
			} while (_ctx.HasNext() && _ctx.CurrentChar == _availableCharacters[ValidCharacters.DefaultsSeparator]);

			if (_ctx.HasNext() && !_availableCharacters.ContainsValue(_ctx.CurrentChar)) throw new Exception();

			return defaults;
		}

		private string GetDefault()
		{
			_ctx.Mark();

			while (_ctx.HasNext() && (char.IsLetter(_ctx.CurrentChar) || char.IsNumber(_ctx.CurrentChar)))
			{
				_ctx.Next();
			}

			_ctx.Previous();
			var val = _ctx.GetMark();
			_ctx.Next();

			if (_ctx.HasNext() && !_availableCharacters.ContainsValue(_ctx.CurrentChar)) throw new Exception();
			return val;
		}

		private (string, Func<object, bool>) ParseConstraint()
		{
			_ctx.Next();
			_ctx.Mark();

			while (_ctx.HasNext() && (char.IsLetter(_ctx.CurrentChar) || char.IsNumber(_ctx.CurrentChar)))
			{
				_ctx.Next();
			}

			_ctx.Previous();
			var val = _ctx.GetMark();
			_ctx.Next();

			if (_availableMatchers.ContainsKey(val))
			{
				return (val, o => Regex.IsMatch((string) o, _availableMatchers[val]));
			}

			if (!_availableParametrizedMatchers.Contains(val) ||
				_ctx.CurrentChar != _availableCharacters[ValidCharacters.NormalOpenBrace]) throw new Exception();

			Func<object, bool> func = null;
			_ctx.Next();
			_ctx.Mark();

			while (_ctx.HasNext() && _ctx.CurrentChar != _availableCharacters[ValidCharacters.NormalClosedBrace])
			{
				_ctx.Next();
			}

			_ctx.Previous();
			var pattern = _ctx.GetMark();
			_ctx.Next();

			if (string.IsNullOrEmpty(pattern) ||
				_ctx.CurrentChar != _availableCharacters[ValidCharacters.NormalClosedBrace]) throw new Exception();

			switch (val)
			{
				case "date":
					func = (o) =>
					{
						DateTime _;
						if (!(o is string str)) return false;
						return DateTime.TryParseExact(str, pattern, null, DateTimeStyles.None, out _);
					};
					break;
				case "inRange":
					if (!pattern.Contains(',')) throw new Exception();
					var args = pattern.Split(',');

					if (args.Length != 2 || args.Any(string.IsNullOrEmpty) || args.Any(s =>
					{
						int _;
						return !int.TryParse(s, out _);
					})) throw new Exception();

					var (arg1, arg2) = (int.Parse(args[0]), int.Parse(args[1]));

					func = o =>
					{
						if (!(o is string toParse) || !int.TryParse(toParse, out var result)) return false;
						return result.InRange(arg1, arg2);
					};

					break;
				case "length":

					if (!(int.TryParse(pattern, out var res))) throw new Exception();

					func = o =>
					{
						if (!(o is string parseable)) return false;
						return parseable.Length <= res;
					};

					break;
				case "max":
					if (!(int.TryParse(pattern, out var res1))) throw new Exception();

					func = o =>
					{
						if (!(o is string parseable) || int.TryParse(parseable, out var rValue)) return false;
						return rValue < res1;
					};

					break;
				case "min":

					if (!(int.TryParse(pattern, out var minRange)))
					{
						throw new Exception();
					}

					func = o =>
					{
						if (!(o is string parseable1) || int.TryParse(parseable1, out var rValue1)) return false;
						return minRange < rValue1;
					};

					break;
			}

			return (val, func);
		}
	}
}