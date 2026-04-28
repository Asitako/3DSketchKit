# Генератор prefab персонажа

Откройте генератор через меню `3D Sketch Kit/Characters/Character Prefab Generator`.

## Входные данные

- Prefab модели или ассет модели.
- Опционально: материал.
- Опционально: базовая текстура.
- Опционально: Animator Controller.
- Шаблон: `NeutralShell`, `PlayerReady`, `MobReady`, `NpcReady`.
- Папка вывода prefab.
- Папка для сгенерированных профилей.
- Имя prefab.

## Результат

Генератор создаёт prefab со следующим каркасом:

- `CharacterEntity`
- `AbilityManager`
- `MovementComponent`
- `HealthComponent`
- `Model`
- `Sockets`
- `Systems`
- `Colliders`

Также создаются минимальные профили и связываются через `CharacterPreset`.

Исходный mesh и rig подключаются по ссылке, без дублирования.

## Руководство для начинающих: полный проход от пустой папки до Shell prefab

### Шаг 1 — подготовьте импорт арта

Следуйте [`SourceMaterialContract_RU.md`](SourceMaterialContract_RU.md):

- импортируйте FBX/GLB;
- настройте Rig/Avatar до генерации игровой оболочки.

Быстрая проверка:

- перетащите модель в пустую Scene и убедитесь, что персонаж виден и масштаб адекватен.

### Шаг 2 — создайте минимальный «набор окружения» в проекте

Минимально полезный набор:

1. текстуры в `Textures/`;
2. материал в `Materials/` (назначить текстуры);
3. `Animator Controller` хотя бы с `Idle`;
4. (опционально) отдельные файлы анимаций в `Animations/`.

Можно без Animator Controller, если вы сознательно делаете статичный прототип без анимации.

### Шаг 3 — откройте окно генератора

Меню:

`3D Sketch Kit → Characters → Character Prefab Generator`

Поля:

- **Model Prefab/Asset**: выберите импортированную модель (или prefab-вариант, который вы сделали от модели).
- **Material / Base Texture**: опционально; удобно, если хотите быстро назначить базовый материал/цвет.
- **Animator Controller**: опционально, но очень рекомендуется.
- **Template**: выберите подсказку роли (`NeutralShell` — самый безопасный дефолт).
- **Output Folder**: по умолчанию `Assets/3DSketchKit/Prefabs/Characters/Shells`, можно заменить на пользовательскую папку.
- **Profile Folder**: можно направить в `Assets/UserContent/Characters/<Name>/Profiles/`, чтобы профили лежали рядом с контентом.

### Шаг 4 — Validate → Generate

1. Нажмите **Validate** и прочитайте ошибки/предупреждения.
2. Сначала исправьте критическое: отсутствие Renderer, розовые материалы, явно сломанный Rig.
3. Нажмите **Generate Character Shell Prefab**.

Генератор создаст:

- shell prefab с host-системами и базовыми компонентами;
- минимальные профили и связку через `CharacterPreset`.

### Шаг 5 — откройте результат и проверьте связи

Откройте созданный prefab и убедитесь:

- внутри `Model` стоит ссылка на ваш импорт (без дублирования mesh/rig как новых файлов);
- при необходимости Animator Controller назначен на Animator внутри модели или на корне (в зависимости от вашего пайплайна и того, как генератор собрал объект).

### Шаг 6 — варианты без дублирования арта

Рекомендуется:

1. ПКМ по shell prefab → `Create → Prefab Variant`.
2. Переименуйте (`PF_<Name>_Player`, `PF_<Name>_Mob`).
3. Меняйте профили/модули/контроль — не меняйте сам FBX.

### Шаг 7 — добавление модулей «правильным» способом

Используйте:

`3D Sketch Kit → Characters → Integrate Character Module`

И подключайте модули через профили (`LocomotionProfile`, `CombatProfile`, ...), чтобы конфигурации можно было переиспользовать.

См. также:

- [`WritingCharacterModules_RU.md`](WritingCharacterModules_RU.md)
- [`SystemHostsArchitecture_RU.md`](SystemHostsArchitecture_RU.md)
