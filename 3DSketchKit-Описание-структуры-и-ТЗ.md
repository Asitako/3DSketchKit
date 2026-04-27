# 3D Sketch Kit — описание структуры ассета и соответствие ТЗ

Документ описывает текущую файловую структуру `Assets/3DSketchKit/`, назначение модулей и **каждого** `.cs` файла, перечень **заглушек**, а также сопоставление с техническим заданием (MVP).

Версия Unity-проекта на момент разработки: **6000.3.x** (см. `ProjectSettings/ProjectVersion.txt`).

---

## 1. Дерево каталогов (фактическое состояние)

```
Assets/3DSketchKit/
├── README.md                      # Quick start + расширение / IL2CPP
├── link.xml                       # IL2CPP: preserve сборки Runtime
├── _Demo/                         # Демо-скрипты (минимум)
├── Documentation/                 # Зарезервировано под PDF/HTML (сейчас пусто + .gitkeep)
├── Editor/                        # Редакторский код (отдельная сборка)
│   ├── Inspectors/
│   └── Windows/
├── Prefabs/                       # Заготовка под префабы (пустые папки + .gitkeep)
│   ├── BuildingBlocks/
│   │   ├── Source/                # опционально: FBX/OBJ рампы для меню Generate Building Block Prefabs
│   │   ├── Meshes/                # процедурный Ramp_Mesh.asset, если Source пуст
│   │   └── Materials/
│   ├── Characters/
│   ├── Zones/
│   └── UI/
└── Runtime/                       # Игровой код (отдельная сборка)
    ├── Core/
    │   ├── Attributes/            # SketchKitAbilityId
    │   ├── Bootstrap/             # RuntimeInitialize каталога способностей
    │   ├── Components/            # MonoBehaviour-менеджеры
    │   ├── Data/                  # ScriptableObject и сериализуемые данные
    │   ├── Interfaces/            # Публичные контракты
    │   └── SketchKitBuiltInAbilityIds.cs
    ├── Modules/
    │   ├── Abilities/
    │   ├── Effects/               # Только заглушка каталога
    │   ├── Rooms/
    │   └── Zones/
    └── Utility/
```

Сборки:

| Файл                                     | Назначение                                                                  |
| ---------------------------------------- | --------------------------------------------------------------------------- |
| `Runtime/ThreeDSketchKit.Runtime.asmdef` | Корневая сборка рантайма, `rootNamespace`: `ThreeDSketchKit`                |
| `Editor/ThreeDSketchKit.Editor.asmdef`   | Редактор; ссылается только на `ThreeDSketchKit.Runtime`, платформа `Editor` |

---

## 2. Назначение верхнеуровневых модулей

| Область                     | Роль                                                                                     |
| --------------------------- | ---------------------------------------------------------------------------------------- |
| **Runtime/Core/Interfaces** | Контракты между менеджерами и «чистыми» модулями; без MonoBehaviour.                     |
| **Runtime/Core/Data**       | Настройки в виде `ScriptableObject` и рантайм-DTO (`EffectData`).                        |
| **Runtime/Core/Components** | Мост к Unity: жизненный цикл, коллайдеры, списки на сцене.                               |
| **Runtime/Modules/**        | Конкретная логика способностей, зон, комнат; классы без наследования от `MonoBehaviour`. |
| **Runtime/Utility**         | Фабрики, отражение типов, вспомогательная инфраструктура.                                |
| **Editor/**                 | Окно меню `Window > 3D Sketch Kit`, кастомный инспектор `AbilityManager`.                |
| **_Demo/**                  | Пример ввода через legacy `Input` (не обязателен для ассета).                            |

---

## 3. Каждый `.cs` файл: путь и назначение

### 3.1. Интерфейсы — `Runtime/Core/Interfaces/`

| Файл                   | Назначение                                                                                   |
| ---------------------- | -------------------------------------------------------------------------------------------- |
| `IAbility.cs`          | Базовый контракт способности: имя, `IsActive`, `PerformAction()`.                            |
| `ITickableAbility.cs`  | Расширение для покадровой логики (например ходьба); вызывается из `AbilityManager.Update`.   |
| `IAbilityHost.cs`      | Контекст для модулей: владелец `GameObject`, `GetDependency<T>()`, вектор ввода перемещения. |
| `IAbilityLifecycle.cs` | Хук `OnAttached(host, AbilityData)` после создания экземпляра способности.                   |
| `IDamageable.cs`       | Здоровье, урон, событие смерти.                                                              |
| `IEffectable.cs`       | Наложение/снятие эффектов по `EffectData`.                                                   |
| `IMovable.cs`          | Скорость и желаемая скорость для мотора.                                                     |
| `IRoom.cs`             | Комната как контейнер: члены, активация/деактивация/уничтожение.                             |
| `IRoomMember.cs`       | Связь объекта с владеющей комнатой (`OwnerRoom`).                                            |
| `IZoneEffect.cs`       | Контракт чистого эффекта зоны: Enter / Stay / Exit.                                          |
| `ZoneEffectSubject.cs` | Обёртка над `GameObject` + `Collider` для передачи в эффекты зоны.                           |

### 3.2. Данные — `Runtime/Core/Data/`

| Файл                | Назначение                                                                                                    |
| ------------------- | ------------------------------------------------------------------------------------------------------------- |
| `AbilityData.cs`    | SO: отображаемое имя, иконка, кулдаун, список параметров `AbilityParameter` + хелпер `GetFloat`.              |
| `EffectData.cs`     | Сериализуемый DTO эффекта для `IEffectable` (id, длительность, сила) + `Clone()`.                             |
| `ZoneEffectData.cs` | SO: вид зоны (урон / стелс / бафф), интервал тика, сила, длительность, префаб VFX, шаблон баффа, слой стелса. |
| `RoomData.cs`       | SO: идентификатор комнаты, правило уничтожения (`DestroyGameObjects` / `DeactivateOnly`).                     |

### 3.3. Компоненты (менеджеры) — `Runtime/Core/Components/`

| Файл                         | Назначение                                                                                                                   |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| `AbilityManager.cs`          | Слоты способностей (`abilityId` и/или assembly-qualified имя), разрешение через `AbilityTypeCatalog`, создание через `Activator`, `Tick`/`Perform*`, `IAbilityHost`. |
| `HealthComponent.cs`         | Реализация `IDamageable` на `MonoBehaviour`.                                                                                 |
| `MovementComponent.cs`       | Реализация `IMovable`: приоритет Rigidbody → CharacterController → сдвиг `Transform`.                                        |
| `EffectReceiverComponent.cs` | Простая реализация `IEffectable` (список активных `EffectData`).                                                             |
| `Room.cs`                    | Реализация `IRoom`; сериализованный список `RoomMemberComponent` + read-only адаптер к `IRoomMember`.                        |
| `RoomMemberComponent.cs`     | Маркер члена комнаты + `IRoomMember`.                                                                                        |
| `ZoneTrigger.cs`             | Триггер-коллайдер; по списку `ZoneEffectData` строит `IZoneEffect` и проксирует Unity trigger-события.                       |
| `RoomZoneAction.cs`          | Отдельный триггер: при входе выполняет `Activate` / `Deactivate` / `Destroy` у выбранной `Room`.                             |

### 3.4. Модули способностей — `Runtime/Modules/Abilities/`

| Файл                    | Назначение                                                                                                               |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `WalkAbility.cs`        | `ITickableAbility`, `[SketchKitAbilityId]`: кэш `Camera.main` для осей движения, пишет в `IMovable`. |
| `JumpAbility.cs`        | `[SketchKitAbilityId]`, импульс прыжка: `Rigidbody.linearVelocity` по Y или запасной вариант через `IMovable`.            |
| `MeleeAttackAbility.cs` | `[SketchKitAbilityId]`, `OverlapSphere`: урон по `HealthComponent` в радиусе, исключая владельца.                         |

### 3.5. Модули зон — `Runtime/Modules/Zones/`

| Файл                   | Назначение                                                                       |
| ---------------------- | -------------------------------------------------------------------------------- |
| `DamageZoneEffect.cs`  | Периодический урон по `HealthComponent` с учётом интервала тика.                 |
| `StealthZoneEffect.cs` | Временная смена `layer` на заданный и восстановление при выходе.                 |
| `BuffZoneEffect.cs`    | При входе `ApplyEffect`, при выходе `RemoveEffect` на `EffectReceiverComponent`. |

### 3.6. Модули комнат — `Runtime/Modules/Rooms/`

| Файл                      | Назначение                                                              |
| ------------------------- | ----------------------------------------------------------------------- |
| `RoomBoundsCalculator.cs` | Статический расчёт `Bounds` по рендерерам и коллайдерам членов комнаты. |

### 3.7. Утилиты — `Runtime/Utility/`

| Файл                      | Назначение                                                                            |
| ------------------------- | ------------------------------------------------------------------------------------- |
| `AbilityTypeCatalog.cs` | Реестр/разрешение типов `IAbility` по стабильному id, FullName, assembly-qualified; `Register`, `RefreshDiscoveredAbilities`. |
| `AbilityTypeDiscovery.cs` | Поиск классов с `IAbility` + `()`; объединяет с типами из каталога (для редактора).   |
| `SketchKitRuntimeLog.cs`  | Условные `LogWarning` только в Editor / Development build.                            |
| `ZoneEffectFactory.cs`    | Создание экземпляра `IZoneEffect` по `ZoneEffectData`.                                |

### 3.7.1. Core — атрибуты, константы, bootstrap

| Файл | Назначение |
|------|------------|
| `Runtime/Core/Attributes/SketchKitAbilityIdAttribute.cs` | Стабильный id класса способности для авто-регистрации. |
| `Runtime/Core/SketchKitBuiltInAbilityIds.cs` | Константы id встроенных способностей. |
| `Runtime/Core/Bootstrap/SketchKitAbilityCatalogBootstrap.cs` | `RuntimeInitializeOnLoad`: вызов `AbilityTypeCatalog.RefreshDiscoveredAbilities()`. |

### 3.8. Редактор — `Editor/`

| Файл                                 | Назначение                                                                    |
| ------------------------------------ | ----------------------------------------------------------------------------- |
| `Windows/SketchKitEditorWindow.cs`   | Окно: комната из выделения, слот способности (abilityId + qualified name), кнопки SO, пересбор каталога. |
| `Inspectors/AbilityManagerEditor.cs` | Кнопка принудительного `RebuildAbilities()` в инспекторе.                     |
| `SketchKitAbilityCatalogEditorSync.cs` | `InitializeOnLoad`: синхронизация `AbilityTypeCatalog` в редакторе без Play. |

### 3.9. Демо — `_Demo/`

| Файл                         | Назначение                                                                     |
| ---------------------------- | ------------------------------------------------------------------------------ |
| `SimpleSketchInputDriver.cs` | Подаёт оси Horizontal/Vertical, Jump, Fire1 в `AbilityManager` (legacy Input). |

---

## 4. Заглушки и неполная реализация

### 4.1. Каталоги-заглушки (без кода или контента)

| Путь | Состояние |
|------|-----------|
| `Documentation/` | Только `.gitkeep`; PDF/HTML из ТЗ **не** добавлены. |
| `Prefabs/*` | Пустые папки + `.gitkeep`; префабы стен/персонажей/зон/UI **отсутствуют**. |
| `Runtime/Modules/Effects/` | Только `.gitkeep`; отдельные модули «бафф/дебафф логика» из ТЗ **не** заведены (кроме минимального хранения в `EffectReceiverComponent`). |

### 4.2. Поля и идеи из ТЗ, пока не используемые в коде

| Элемент                            | Комментарий                                                                                                                                    |
| ---------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| `AbilityData` — иконка, кулдаун    | Заданы в SO, **нет** логики кулдауна и UI в рантайме.                                                                                          |
| `ZoneEffectData.Duration`          | В фабрике и эффектах **не** обрабатывается (нет таймера зоны по длительности).                                                                 |
| `ZoneEffectData.VisualPrefab`      | **Не** инстанциируется из `ZoneTrigger`.                                                                                                       |
| `RoomData.RoomId`                  | Хранится в SO, в логике `Room` **не** используется (только `DestroyRule`).                                                                     |
| `MeleeAttackAbility` — маска слоёв | Поле задумывалось под конфиг; в коде используется `~0`.                                                                                        |
| `MovementComponent`                | **Нет** `NavMeshAgent` (в ТЗ упоминался как вариант).                                                                                          |
| Окно редактора                     | **Нет** полноценного UI «привязка триггера к действию с комнатой» — для этого есть компонент `RoomZoneAction`, настройка вручную в инспекторе. |

### 4.3. Архитектурные ограничения MVP

- Модули в `Modules/` **могут** ссылаться на `UnityEngine` (физика, слои); строгое «без Unity API» из ТЗ для MVP ослаблено на практике.
- Расширение пользователем: `AbilityTypeCatalog` (атрибут `[SketchKitAbilityId]` + автоскан загруженных сборок, либо явный `Register`), слот `abilityId` или assembly-qualified строка; для IL2CPP — свой `link.xml` / `Register` на сборке игры.

---

## 5. Соответствие техническому заданию (сводка)

Условные обозначения: **да** — есть и работает в базовом виде; **частично** — есть задел или упрощение; **нет** — не делалось.

| Требование ТЗ                                                    | Статус                                                                                        |
| ---------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| Модульность поведения (`IAbility`, модули без `MonoBehaviour`)   | **да**                                                                                        |
| Расширяемость без правки ассета (свои типы + интерфейсы + слоты) | **да** (`AbilityTypeCatalog`, `[SketchKitAbilityId]`, автоскан при старте; явный `Register`; `link.xml` для IL2CPP; fallback `Type.GetType`) |
| Стартовый набор способностей (Walk, Jump, Melee)                 | **да**                                                                                        |
| Стартовый набор зон (урон, стелс, бафф)                          | **да**                                                                                        |
| Зоны как триггер + чистые эффекты                                | **да** (`ZoneTrigger` + `IZoneEffect`)                                                        |
| Комнаты: владелец, список членов, массовые операции              | **да**                                                                                        |
| Триггеры → действия с комнатой                                   | **частично** (`RoomZoneAction`; без визуального редактора привязок)                           |
| Единое окно редактора                                            | **частично** (есть `Window > 3D Sketch Kit`, без полного набора из ТЗ)                        |
| Данные в `ScriptableObject`                                      | **да** (`AbilityData`, `ZoneEffectData`, `RoomData`)                                          |
| Общение через интерфейсы                                         | **да** (ядро завязано на интерфейсы)                                                          |
| Демо-сцена `_Demo/`                                              | **частично** (только вспомогательный скрипт ввода, без готовой сцены)                         |
| Документация `Documentation/`                                    | **нет** (плейсхолдер)                                                                         |
| Префабы `Prefabs/`                                               | **нет** (плейсхолдеры папок)                                                                  |
| Требования Asset Store Submission Guidelines                     | **частично** (структура и разделение Editor/Runtime соблюдены; финальный аудит не проводился) |

---

## 6. Связь ключевых типов (краткая схема)

```
GameObject + AbilityManager ──► создаёт IAbility (модули из Modules/Abilities)
              │                      ▲
              ├── IAbilityHost       │ OnAttached / Tick / PerformAction
              ├── MovementComponent (IMovable)
              ├── HealthComponent (IDamageable)
              └── (опционально) EffectReceiverComponent (IEffectable)

GameObject + Room ◄── IRoom ◄── RoomMemberComponent (IRoomMember)

Collider isTrigger + ZoneTrigger ──► ZoneEffectFactory(ZoneEffectData) ──► IZoneEffect
```

---

*Документ можно обновлять по мере добавления префабов, демо-сцены, PDF и новых модулей.*
