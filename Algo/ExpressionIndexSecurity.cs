#region S# License
/******************************************************************************************
NOTICE!!!  This program and source code is owned and licensed by
StockSharp, LLC, www.stocksharp.com
Viewing or use of this code requires your acceptance of the license
agreement found at https://github.com/StockSharp/StockSharp/blob/master/LICENSE
Removal of this comment is a violation of the license agreement.

Project: StockSharp.Algo.Algo
File: ExpressionIndexSecurity.cs
Created: 2015, 11, 11, 2:32 PM

Copyright 2010 by StockSharp, LLC
*******************************************************************************************/
#endregion S# License
namespace StockSharp.Algo
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;

	using Ecng.Collections;
	using Ecng.Configuration;
	using Ecng.Serialization;

	using NCalc;

	using StockSharp.Algo.Storages;
	using StockSharp.BusinessEntities;
	using StockSharp.Localization;

	#region Ignore
	//[Ignore(FieldName = "Code")]
	/// <summary>
	/// The index, built of combination of several instruments through mathematic formula <see cref="ExpressionIndexSecurity.Expression"/>.
	/// </summary>
	[Ignore(FieldName = "Class")]
	[Ignore(FieldName = "Name")]
	[Ignore(FieldName = "ShortName")]
	//[Ignore(FieldName = "Board")]
	[Ignore(FieldName = "ExtensionInfo")]
	[Ignore(FieldName = "Decimals")]
	[Ignore(FieldName = "VolumeStep")]
	[Ignore(FieldName = "PriceStep")]
	[Ignore(FieldName = "StepPrice")]
	[Ignore(FieldName = "OpenPrice")]
	[Ignore(FieldName = "ClosePrice")]
	[Ignore(FieldName = "HighPrice")]
	[Ignore(FieldName = "LowPrice")]
	[Ignore(FieldName = "MaxPrice")]
	[Ignore(FieldName = "MinPrice")]
	[Ignore(FieldName = "MarginBuy")]
	[Ignore(FieldName = "MarginSell")]
	[Ignore(FieldName = "Type")]
	[Ignore(FieldName = "OptionType")]
	[Ignore(FieldName = "TheorPrice")]
	[Ignore(FieldName = "Volatility")]
	[Ignore(FieldName = "Strike")]
	[Ignore(FieldName = "UnderlyingSecurityId")]
	[Ignore(FieldName = "OpenInterest")]
	[Ignore(FieldName = "SettlementDate")]
	[Ignore(FieldName = "ExpiryDate")]
	[Ignore(FieldName = "State")]
	[Ignore(FieldName = "LastTrade")]
	[Ignore(FieldName = "BestBid")]
	[Ignore(FieldName = "BestAsk")]
	[Ignore(FieldName = "Currency")]
	[Ignore(FieldName = "Sedol")]
	[Ignore(FieldName = "Cusip")]
	[Ignore(FieldName = "Isin")]
	[Ignore(FieldName = "Ric")]
	[Ignore(FieldName = "Bloomberg")]
	[Ignore(FieldName = "IQFeed")]
	[Ignore(FieldName = "LastChangeTime")]
	[Ignore(FieldName = "InteractiveBrokers")]
	[Ignore(FieldName = "Plaza")]
	#endregion
	[DisplayNameLoc(LocalizedStrings.IndexKey)]
	[DescriptionLoc(LocalizedStrings.Str728Key)]
	public class ExpressionIndexSecurity : IndexSecurity
	{
		private readonly SynchronizedList<Security> _innerSecurities = new SynchronizedList<Security>(); 

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionIndexSecurity"/>.
		/// </summary>
		public ExpressionIndexSecurity()
		{
			Board = ExchangeBoard.Associated;
		}

		private string _expressionText;
		private Expression _expression;

		/// <summary>
		/// The mathematic formula of index.
		/// </summary>
		[Browsable(false)]
		public string Expression
		{
			get { return _expressionText; }
			set
			{
				_expressionText = value;
				_expression = new Expression(ExpressionHelper.Encode(value));

				_innerSecurities.Clear();
				_expression.Parameters.Clear();

				if (!_expression.HasErrors())
				{
					var registry = ConfigManager.GetService<IEntityRegistry>();

					foreach (var id in _expression.GetSecurityIds())
					{
						_expression.Parameters[id] = null;

						var security = registry.Securities.ReadById(id);

						if (security != null)
							_innerSecurities.Add(security);
					}
				}
			}
		}

		/// <summary>
		/// Instruments, from which this basket is created.
		/// </summary>
		public override IEnumerable<Security> InnerSecurities
		{
			get { return _innerSecurities.SyncGet(c => c.ToArray()); }
		}

		/// <summary>
		/// To calculate the basket value.
		/// </summary>
		/// <param name="prices">Prices of basket composite instruments <see cref="BasketSecurity.InnerSecurities"/>.</param>
		/// <returns>The basket value.</returns>
		public override decimal? Calculate(IDictionary<Security, decimal> prices)
		{
			if (prices == null)
				throw new ArgumentNullException(nameof(prices));

			if (prices.Count != _expression.Parameters.Count || !_innerSecurities.All(prices.ContainsKey))
				return null;

			foreach (var pair in prices)
			{
				_expression.Parameters[pair.Key.Id] = (double)pair.Value;
			}

			var value = (double)_expression.Evaluate();
			return value.ToDecimal();
		}

		/// <summary>
		/// Create a copy of <see cref="Security"/>.
		/// </summary>
		/// <returns>Copy.</returns>
		public override Security Clone()
		{
			var clone = new ExpressionIndexSecurity { Expression = Expression };
			CopyTo(clone);
			return clone;
		}
	}
}