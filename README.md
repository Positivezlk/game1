# Neon Dash Yandex

Unity WebGL 2D endless runner / arcade, подготовленный под публикацию на Яндекс Играх.

## Что реализовано

- Unity WebGL проект со сценой `Assets/Scenes/Main.unity` и runtime-сборкой 2D runner-геймплея.
- Интеграция под Yandex Games SDK через Plugin Your Games 2.0:
  - код вызывает `YG2.GameReadyAPI()` только после создания мира, UI, загрузки сохранений и показа главного меню;
  - код вызывает `YG2.GameplayStart()` / `YG2.GameplayStop()` при старте, паузе, Game Over и рекламе;
  - код вызывает `YG2.InterstitialAdvShow()` только на Game Over и каждые 180 секунд как завершение условного этапа;
  - код вызывает `YG2.RewardedAdvShow("coins_25", callback)` только по кнопке с текстом награды;
  - добавлен WebGL fallback `Assets/Plugins/WebGL/YandexBridge.jslib` на случай сборки без импортированного PluginYG2.
- Пауза игры и звука на fullscreen/rewarded рекламе, восстановление после закрытия.
- Сохранение рекорда, монет, выбранного скина, миссий и числа забегов.
- Автоопределение языка SDK/браузера, RU/EN тексты, переключатель языка в настройках.
- Адаптивный Canvas `Scale With Screen Size`, запрет браузерного скролла в WebGL template.
- Управление: Space/Up/mouse на desktop, tap на mobile.
- Главное меню, пауза, Game Over, настройки, экран управления.
- Длинная сессия: бесконечный раннер с ростом скорости, тремя типами препятствий, монетами, миссиями и рекламным брейком после завершения условного этапа.

## Требования к Unity

- Рекомендуемая версия: Unity `2022.3 LTS` или новее.
- Target Platform: `WebGL`.
- Compression: Brotli/Gzip допустимы; для внешнего хостинга можно временно включить Decompression Fallback.
- Размер распакованного билда должен оставаться меньше 100 МБ: проект не содержит тяжелых ассетов, все визуальные элементы создаются кодом из встроенных примитивов.

## Установка Plugin Your Games 2.0

1. Откройте проект в Unity.
2. Убедитесь, что в проекте нет ошибок компиляции до импорта плагина.
3. Установите `Plugin Your Games 2.0` из Unity Asset Store или официальной страницы PluginYG2.
4. В настройках PluginYG2 выберите платформу `Yandex Games`.
5. Импортируйте модули:
   - `InterstitialAdv`;
   - `RewardedAdv`;
   - `Storage`;
   - `EnvirData` или `Localization`.
6. Отключите `Auto GRA`, потому что игра вызывает Game Ready вручную после полной готовности меню.
7. Проверьте, что WebGL Template выбран как `PROJECT:YandexGames` или шаблон PluginYG2 для Yandex Games. Если используете шаблон PluginYG2, перенесите CSS-правила запрета скролла из `Assets/WebGLTemplates/YandexGames/index.html`.

## Как собрать WebGL

1. `File -> Build Settings -> WebGL -> Switch Platform`.
2. Добавьте сцену `Assets/Scenes/Main.unity` в Build Settings, если Unity не добавила её автоматически.
3. `Player Settings -> Resolution and Presentation -> WebGL Template`: выберите `YandexGames`.
4. `Player Settings -> Publishing Settings`: включите сжатие, подходящее для вашего хостинга.
5. Нажмите `Build` и выберите папку без пробелов и русских символов, например `Builds/NeonDashYandex`.
6. Для архивации выделите содержимое папки билда так, чтобы `index.html` лежал в корне zip-архива, рядом с папками `Build` и `StreamingAssets`.

## Как проверить SDK

1. Соберите WebGL билд.
2. Запустите через локальный web server или через режим тестирования PluginYG2/Яндекс Игр.
3. Откройте DevTools Console и проверьте:
   - нет ошибок загрузки `/sdk.js` в окружении Яндекс Игр;
   - после появления главного меню вызывается `LoadingAPI.ready()` / `YG2.GameReadyAPI()`;
   - при старте забега отправляется `GameplayStart`;
   - при паузе, Game Over и рекламе отправляется `GameplayStop`;
   - после закрытия рекламы gameplay возобновляется.
4. В DevTools Performance/Game Ready Яндекс Игр убедитесь, что Game Ready отмечается только после исчезновения загрузочного состояния и готовности меню.

## Как тестировать рекламу

- Interstitial:
  1. Начните забег.
  2. Столкнитесь с препятствием — interstitial вызывается на Game Over.
  3. Продержитесь около 180 секунд — interstitial вызывается после завершения условного этапа.
- Rewarded:
  1. На экране Game Over нажмите кнопку `Смотреть видео: +25 монет` / `Watch video: +25 coins`.
  2. Награда начисляется только из callback успешного rewarded video.
- Во время fullscreen/rewarded рекламы:
  - `Time.timeScale = 0`;
  - `AudioListener.pause = true`;
  - после закрытия рекламы состояние и звук восстанавливаются.

## Проверка требований модерации Яндекс Игр

- Не подключайте сторонние рекламные SDK. Используйте только Yandex Games SDK / PluginYG2.
- Не показывайте interstitial на старте игры или во время активного геймплея без логической паузы.
- Rewarded video доступен только по явной кнопке с описанием награды.
- Проверьте mobile viewport: кнопки не должны обрезаться, страница не должна скроллиться, canvas не должен растягиваться неравномерно.
- Проверьте desktop и mobile управление.
- Проверьте сохранения после обновления страницы: рекорд, монеты и выбранный скин должны восстановиться.
- Убедитесь, что в именах файлов/папок билда нет пробелов и русских символов.
