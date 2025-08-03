using NinjaTrader.Cbi;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Configs;
using NinjaTrader.Custom.AddOns.DiscordMessenger.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using OrderEntry = NinjaTrader.Custom.AddOns.DiscordMessenger.Models.OrderEntry;
using Position = NinjaTrader.Custom.AddOns.DiscordMessenger.Models.Position;

namespace NinjaTrader.Custom.AddOns.DiscordMessenger.Services
{
    public class TradingStatusService
    {
        private readonly TradingStatusEvents _tradingStatusEvents;
        private Account _account;
        private List<Position> _positions;
        private List<OrderEntry> _orderEntries;

        public TradingStatusService(TradingStatusEvents tradingStatusEvents)
        {
            _tradingStatusEvents = tradingStatusEvents;
            _tradingStatusEvents.OnManualOrderEntryUpdate += HandleOrderEntryUpdated;
            _tradingStatusEvents.OnOrderEntryUpdated += HandleManualOrderEntryUpdated;
            _tradingStatusEvents.OnOrderEntryUpdatedSubscribe += HandleOnOrderEntryUpdatedSubscribe;
            _tradingStatusEvents.OnOrderEntryUpdatedUnsubscribe += HandleOnOrderEntryUpdatedUnsubscribe;

            _account = Config.Instance.Account;

            _positions = new List<Position>();
            _orderEntries = new List<OrderEntry>();
        }

        private void HandleOnOrderEntryUpdatedSubscribe()
        {
            _tradingStatusEvents.OnOrderEntryUpdated += HandleOrderEntryUpdated;
        }

        private void HandleOnOrderEntryUpdatedUnsubscribe()
        {
            _tradingStatusEvents.OnOrderEntryUpdated -= HandleOrderEntryUpdated;
        }

        private void HandleManualOrderEntryUpdated()
        {
            HandleOrderEntryUpdated();
        }

        private void HandlePositionUpdated()
        {
            int totalPositions = _account.Positions.Count;
            _positions = new List<Position>();

            // No position
            if (totalPositions == 0)
            {
                return;
            }

            for (int i = 0; i < totalPositions; i++)
            {
                Position currentPosition = new Position
                {
                    Instrument = _account.Positions[i].Instrument.MasterInstrument.Name,
                    Quantity = _account.Positions[i].Quantity,
                    AveragePrice = Math.Round(_account.Positions[i].AveragePrice, 2),
                    MarketPosition = _account.Positions[i].MarketPosition.ToString(),
                };

                _positions.Add(currentPosition);
            }
        }

        private void HandleOrderEntryUpdated()
        {
            int totalOrders = _account.Orders.Count;
            _orderEntries = new List<OrderEntry>();

            for (int i = 0; i < totalOrders; i++)
            {
                if (
                    _account.Orders[i].OrderState != OrderState.Accepted &&
                    _account.Orders[i].OrderState != OrderState.Working
                )
                {
                    continue;
                }

                double price;

                // Check for proper price for limit order since order may have a price for both
                if (
                    _account.Orders[i].OrderType == OrderType.StopLimit ||
                    _account.Orders[i].OrderType == OrderType.StopMarket ||
                    _account.Orders[i].OrderType == OrderType.MIT
                )
                {
                    price = _account.Orders[i].StopPrice;
                }
                else
                {
                    price = _account.Orders[i].LimitPrice;
                }

                // Check if an order with the same type and price already exists
                OrderEntry existingOrder = null;
                foreach (var entry in _orderEntries)
                {
                    if (entry.Type == _account.Orders[i].OrderType.ToString() && entry.Price == price)
                    {
                        existingOrder = entry;
                        break;
                    }
                }

                if (existingOrder != null)
                {
                    // Update the quantity if a matching order is found
                    existingOrder.Quantity += _account.Orders[i].Quantity;
                }
                else
                {
                    // Add new order entry if no match is found
                    OrderEntry orderEntry = new OrderEntry
                    {
                        Instrument = _account.Orders[i].Instrument.MasterInstrument.Name,
                        Quantity = _account.Orders[i].Quantity,
                        Price = Math.Round(price, 2),
                        Type = _account.Orders[i].OrderType.ToString(),
                        Action = _account.Orders[i].OrderAction.ToString()
                    };

                    _orderEntries.Add(orderEntry);
                }
            }

            // Sort descending order by price so it appears natural to the chart
            _orderEntries.Sort(delegate(OrderEntry a, OrderEntry b) 
            { 
                return b.Price.CompareTo(a.Price); 
            });

            HandlePositionUpdated();

            _tradingStatusEvents.OrderEntryProcessed(_positions, _orderEntries);
        }
    }
}