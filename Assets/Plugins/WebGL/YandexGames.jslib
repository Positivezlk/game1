mergeInto(LibraryManager.library, {
  YG_ShowRewarded: function(callbackObjectPtr) {
    var callbackObject = UTF8ToString(callbackObjectPtr);

    if (typeof ysdk === 'undefined' || !ysdk.adv) {
      unityInstance.SendMessage(callbackObject, 'OnRewardedFailed');
      return;
    }

    ysdk.adv.showRewardedVideo({
      callbacks: {
        onOpen: function() {
          // Пауза уже выставляется в C#.
        },
        onRewarded: function() {
          unityInstance.SendMessage(callbackObject, 'OnRewardedSuccess');
        },
        onClose: function() {
          // Ничего, игра продолжится после OnRewardedSuccess/Failed.
        },
        onError: function() {
          unityInstance.SendMessage(callbackObject, 'OnRewardedFailed');
        }
      }
    });
  }
});
