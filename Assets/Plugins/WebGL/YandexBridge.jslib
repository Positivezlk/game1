mergeInto(LibraryManager.library, {
  YGBridgeInit: function (gameObjectPtr) {
    const gameObject = UTF8ToString(gameObjectPtr);
    window.NeonDashYandex = window.NeonDashYandex || {};
    window.NeonDashYandex.gameObject = gameObject;
    if (!window.NeonDashYandex.initPromise) {
      window.NeonDashYandex.initPromise = (window.YaGames ? window.YaGames.init() : Promise.resolve(null))
        .then(function (ysdk) { window.NeonDashYandex.ysdk = ysdk; return ysdk; })
        .catch(function (error) { console.warn('YaGames init failed', error); return null; });
    }
  },
  YGBridgeReady: function () {
    const api = window.NeonDashYandex;
    (api && api.initPromise ? api.initPromise : Promise.resolve(null)).then(function (ysdk) {
      if (ysdk && ysdk.features && ysdk.features.LoadingAPI) ysdk.features.LoadingAPI.ready();
    });
  },
  YGBridgeGameplayStart: function () {
    const ysdk = window.NeonDashYandex && window.NeonDashYandex.ysdk;
    if (ysdk && ysdk.features && ysdk.features.GameplayAPI) ysdk.features.GameplayAPI.start();
  },
  YGBridgeGameplayStop: function () {
    const ysdk = window.NeonDashYandex && window.NeonDashYandex.ysdk;
    if (ysdk && ysdk.features && ysdk.features.GameplayAPI) ysdk.features.GameplayAPI.stop();
  },
  YGBridgeShowInterstitial: function (reasonPtr) {
    const api = window.NeonDashYandex;
    const close = function () { if (api && api.unityInstance) api.unityInstance.SendMessage(api.gameObject, 'OnInterstitialClosedFromJs'); };
    (api && api.initPromise ? api.initPromise : Promise.resolve(null)).then(function (ysdk) {
      if (ysdk && ysdk.adv) ysdk.adv.showFullscreenAdv({ callbacks: { onClose: close, onError: close } });
      else setTimeout(close, 300);
    });
  },
  YGBridgeShowRewarded: function () {
    const api = window.NeonDashYandex;
    let rewarded = false;
    const send = function (method) { if (api && api.unityInstance) api.unityInstance.SendMessage(api.gameObject, method); };
    (api && api.initPromise ? api.initPromise : Promise.resolve(null)).then(function (ysdk) {
      if (ysdk && ysdk.adv) {
        ysdk.adv.showRewardedVideo({ callbacks: {
          onRewarded: function () { rewarded = true; send('OnRewardedGrantedFromJs'); },
          onClose: function () { if (!rewarded) send('OnRewardedClosedWithoutRewardFromJs'); },
          onError: function () { send('OnRewardedClosedWithoutRewardFromJs'); }
        }});
      } else setTimeout(function () { send('OnRewardedGrantedFromJs'); }, 300);
    });
  },
  YGBridgeGetLanguage: function () {
    const ysdk = window.NeonDashYandex && window.NeonDashYandex.ysdk;
    const lang = ysdk && ysdk.environment && ysdk.environment.i18n ? ysdk.environment.i18n.lang : ((navigator.language || 'en').slice(0, 2));
    const bytes = lengthBytesUTF8(lang) + 1;
    const buffer = _malloc(bytes);
    stringToUTF8(lang, buffer, bytes);
    return buffer;
  },
  YGBridgeLoad: function () {
    const json = localStorage.getItem('neon_dash_save_v1') || '';
    const bytes = lengthBytesUTF8(json) + 1;
    const buffer = _malloc(bytes);
    stringToUTF8(json, buffer, bytes);
    return buffer;
  },
  YGBridgeSave: function (jsonPtr) {
    localStorage.setItem('neon_dash_save_v1', UTF8ToString(jsonPtr));
  }
});
