﻿using BTCPayApp.Core;
using BTCPayApp.Core.Contracts;
using BTCPayApp.UI.Features;
using Fluxor;

namespace BTCPayApp.UI;

public class UIStateMiddleware : Middleware
{
    private readonly IConfigProvider _configProvider;
    private readonly BTCPayAppConfigManager _btcPayAppConfigManager;
    private readonly BTCPayConnection _btcPayConnection;

    public UIStateMiddleware(IConfigProvider configProvider, BTCPayAppConfigManager btcPayAppConfigManager, BTCPayConnection btcPayConnection)
    {
        _configProvider = configProvider;
        _btcPayAppConfigManager = btcPayAppConfigManager;
        _btcPayConnection = btcPayConnection;
    }

    public override async Task InitializeAsync(IDispatcher dispatcher, IStore store)
    {
        if (store.Features.TryGetValue(typeof(UIState).FullName, out var uiStateFeature))
        {
            var existing = await _configProvider.Get<UIState>("uistate");
            if (existing is not null)
            {
                uiStateFeature.RestoreState(existing);
            }

            uiStateFeature.StateChanged += async (sender, args) =>
            {
                await _configProvider.Set("uistate", (UIState) uiStateFeature.GetState());
            };
        }

        await base.InitializeAsync(dispatcher, store);

        ListenIn(dispatcher);
    }

    private void ListenIn(IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new RootState.LoadingAction(true));
        _btcPayAppConfigManager.PairConfigUpdated +=
            (_, config) => dispatcher.Dispatch(new RootState.PairConfigLoadedAction(config));
        _btcPayAppConfigManager.WalletConfigUpdated += (_, config) =>
            dispatcher.Dispatch(new RootState.WalletConfigLoadedAction(config));
        _ = _btcPayAppConfigManager.Loaded.Task.ContinueWith(_ =>
        {
            dispatcher.Dispatch(new RootState.LoadingAction(false));
            dispatcher.Dispatch(new RootState.WalletConfigLoadedAction(_btcPayAppConfigManager.WalletConfig));
            dispatcher.Dispatch(new RootState.PairConfigLoadedAction(_btcPayAppConfigManager.PairConfig));
        });
        _btcPayConnection.ConnectionChanged += (sender, args) =>
            dispatcher.Dispatch(new RootState.BTCPayConnectionUpdatedAction(_btcPayConnection.Connection?.State));
    }
}