# Idle-игра для Яндекс Игр: **«Звёздная Артель»**

## 1) Идея игры

**Жанр:** Idle / Incremental / Clicker  
**Сеттинг:** космический бизнес по добыче руды на астероидах и производству научных модулей.  
**Петля игрока:**
1. Нажимает кнопку «Добыть» и получает кредиты.
2. Покупает апгрейды добычи, фабрик и лабораторий.
3. Доход начинает идти пассивно.
4. Открывает новые технологические этапы (5 тиров).
5. Делает престиж «Перезапуск экспедиции» и ускоряет следующий цикл.

Игра рассчитана на 2–3 часа активного прогресса до стабильных престиж-циклов.

---

## 2) Механики

1. **Клик-доход** — кнопка «Добыть кредиты».
2. **Пассивный доход** — кредиты/руда/наука в секунду.
3. **Система апгрейдов** — 40 апгрейдов (5 паков × 8).
4. **Мультипликаторы** — глобальные, престижные, рекламные.
5. **Прогрессия** — 5 этапов (Tier 1–5).
6. **Престиж (Rebirth)** — сброс прогресса ради постоянного бонуса.
7. **Оффлайн-доход** — начисление за время отсутствия.
8. **Реклама за награду** — буст кредитов за rewarded ad.
9. **Достижения** — 5 базовых достижений (расширяется).
10. **Случайные бонусы** — периодические «космические контейнеры».

---

## 3) Экономика

### 3.1 Ресурсы
- **Кредиты** — основной ресурс (клик + пассив).
- **Руда** — средний ресурс (производится пассивно).
- **Наука** — редкий ресурс для поздних апгрейдов.

### 3.2 Формулы

**Стоимость апгрейда:**
\[
Cost(n) = BaseCost \cdot Growth^{n}
\]

**Суммарный бонус апгрейда с уровнем n:**
\[
Bonus(n)=BaseIncomeBonus\cdot\frac{IncomeGrowth^{n}-1}{IncomeGrowth-1}
\]

**Доход за клик:**
\[
ClickIncome = BaseClick \cdot ClickMultiplier \cdot GlobalMultiplier \cdot PrestigeMultiplier
\]

**Пассивный доход ресурса r:**
\[
Income_r = (Base_r + UpgradeBonus_r) \cdot PassiveMultiplier \cdot GlobalMultiplier \cdot PrestigeMultiplier
\]

**Престиж-множитель:**
\[
PrestigeMultiplier = 1 + 0.08 \cdot PrestigePoints
\]

**Очки престижа при сбросе:**
\[
PrestigeGain = \left\lfloor\sqrt{\frac{Credits + 20\cdot Ore + 200\cdot Science}{10000}}\right\rfloor
\]

**Оффлайн-доход:**
\[
OfflineIncome = IncomePerSecond \cdot OfflineSeconds \cdot 0.65
\]
где `OfflineSeconds <= 8 часов`.

### 3.3 Баланс (пример)

| Этап | Диапазон силы экономики | Ср. доход/сек | Типовые стоимости апгрейдов | Множители |
|---|---:|---:|---:|---|
| 1 | 0 – 15k | 1–25 кредитов | 10–2k кредитов | x1.0–x1.5 |
| 2 | 15k – 120k | 25–180 кредитов | 2k–20k кредитов, 20–200 руды | x1.5–x3 |
| 3 | 120k – 800k | 180–1.4k кредитов | 20k–140k, 200–2k руды, 10–80 науки | x3–x7 |
| 4 | 800k – 5M | 1.4k–9k кредитов | 140k–1.5M + наука | x7–x15 |
| 5 | 5M+ | 9k+ кредитов | 1.5M+ и высокие науки | x15+ и престиж |

---

## 4) Структура проекта Unity


> Сцена `Assets/Scenes/Main.unity` уже готова к запуску: при старте `SceneBootstrapper` автоматически создаёт все менеджеры, Canvas, кнопки, индикаторы и базовую панель магазина без ручной настройки в инспекторе.

```text
Assets/
  Scenes/
    Main.unity (готовая сцена, автосборка UI и систем)
  Scripts/
    Bootstrap/
      SceneBootstrapper.cs
    Data/
      GameData.cs
    Economy/
      UpgradeDefinition.cs
    Managers/
      GameManager.cs
      EconomyManager.cs
      UpgradeSystem.cs
      SaveSystem.cs
      AdManager.cs
    Systems/
      IdleIncomeSystem.cs
      OfflineIncome.cs
      AchievementSystem.cs
      RandomBonusSystem.cs
    UI/
      UIManager.cs
    Yandex/
      YandexGamesBridge.cs
  Plugins/
    WebGL/
      YandexGames.jslib
  Prefabs/
    (UI и игровые префабы)
  UI/
    (иконки, спрайты, шрифты)
```

---

## 5) Код

Готовые C# скрипты:
- `GameManager`
- `EconomyManager`
- `UpgradeSystem`
- `IdleIncomeSystem`
- `SaveSystem`
- `UIManager`
- `AdManager`
- `OfflineIncome`
- + расширения: `AchievementSystem`, `RandomBonusSystem`, `YandexGamesBridge`

Готовый JS plugin:
- `Assets/Plugins/WebGL/YandexGames.jslib`

---

## 6) UI (русский интерфейс)

Минимальный набор элементов на Canvas:
1. Текст: **Кредиты**, **Руда**, **Наука**.
2. Кнопка: **«Добыть»** (клик-доход).
3. ScrollView: **магазин апгрейдов**.
4. Кнопка: **«Реклама: x120 сек дохода»**.
5. Индикатор: **прогресс этапа** (Slider).
6. Блок: **Оффлайн-доход**.
7. Кнопка: **«Престиж»**.

---

## 7) Интеграция рекламы (Yandex Games SDK)

### Пример C# (в проекте)
`AdManager.ShowRewardedAd()` вызывает JS-функцию `YG_ShowRewarded`, ставит игру на паузу, ждёт callback и выдаёт награду в кредитах.

### Пример JS (в проекте)
`YandexGames.jslib` вызывает `ysdk.adv.showRewardedVideo`, и в `onRewarded` отправляет callback в Unity:
```js
unityInstance.SendMessage(callbackObject, 'OnRewardedSuccess');
```

### Что важно
- Пауза игры на время рекламы: `Time.timeScale = 0`.
- Возврат времени после успеха/ошибки.
- Награда выдается только из `onRewarded`.

---

## 8) Экспорт WebGL

1. Открой `File -> Build Settings`.
2. Выбери `WebGL`, нажми `Switch Platform`.
3. В `Player Settings`:
   - Color Space: Gamma (проще для браузеров)
   - Compression: gzip/Brotli
   - Strip Engine Code: On
4. Собери проект: `Build` в папку `Build/WebGL`.

---

## 9) Публикация в Яндекс Игры

1. Зарегистрируйся в кабинете разработчика Яндекс Игр.
2. Создай новую игру.
3. Загрузите WebGL build (index + data/framework/wasm).
4. Подключите SDK Яндекс Игр в шаблоне/обвязке.
5. Проверь rewarded ads и паузу игры.
6. Заполни карточку игры (иконка, описание, возраст).
7. Отправь на модерацию.

---

## Инструкция для новичка (максимально просто)

1. **Установи Unity Hub** с сайта Unity.
2. **Поставь Unity Editor** (LTS версия, например 2022 LTS).
3. **Создай проект**: шаблон `2D Core`, имя `IdleStarforge`.
4. **Скопируй папку Assets** из этого репозитория в проект.
5. **Открой сцену** `Main.unity` (или создай новую и добавь Canvas + EventSystem).
6. **Добавь объект GameBootstrap** и повесь на него менеджеры.
7. **Свяжи ссылки в инспекторе** (кнопки, тексты, слайдер).
8. **Нажми Play** и проверь:
   - клик даёт кредиты,
   - пассивный доход идёт,
   - апгрейды покупаются,
   - реклама даёт награду.
9. **Собери WebGL** через `Build Settings`.
10. **Загрузи билд в Яндекс Игры** и протестируй в черновике.

Если что-то не работает: сначала проверь, что все ссылки на UI-компоненты назначены в инспекторе.
