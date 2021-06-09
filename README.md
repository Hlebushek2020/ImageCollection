# Image Collection
**Image Collection** - программа, которая поможет распределить фотографии (картинки) по соответствующим коллекциям, которые в свою очередь, можно распределить по соответствующим папкам.  
Пользователь предварительно настраивает коллекции, при этом изменения не применяются. Для применения изменений нужно нажать в меню “Коллекция” на пункт “Распределить по папкам”, для сохранения коллекций без распределения нужно нажать в меню “Файл” пункт “Сохранить коллекции”.

При старте программы будет открыто окно “Начало работы”, где пользователю предоставляется два варианта действий “Открыть папку” и “Открыть коллекции”. Так же пользователь может закрыть это окно и выполнить эти действия из соответствующего меню.
## Содержание
- [Требования](#требования)
- [Меню](#меню)
  - [Файл](#файл)
  - [Коллекция](#коллекция)
  - [Опции](#опции)
- [Настройки открытия папки](#настройки-открытия-папки)
- [Интеграция](#интеграция)
## Требования
- [.NET Framework 4.7](https://www.microsoft.com/ru-RU/download/details.aspx?id=55167)
## Меню
### Файл
**Открыть папку** - откроет окно с настройками открытия папки (подробнее в разделе “[Настройки открытия папки](#настройки-открытия-папки)”).  
**Открыть коллекции** - загрузит все коллекции в указанной директории.  
**Сохранить коллекции** - сохранит все сведения о коллекциях (сведения хранятся в специальной папке, которая создается в открытой папке).  
**Удалить** - удаляет текущий выбранный файл без возможности восстановления.  
**Удалить выбранные** - удаляет текущие выбранные файлы без возможности восстановления.  
**Переименовать** - переименовать текущий выбранный в списке файл.  
**Добавить коллекцию...** - откроет окно для выбора коллекции, в которую будет добавлено текущее изображение.
### Коллекция
**Создать** - открывает окно для ввода названия новой коллекции и описания (необязательно).  
**Изменить** - открывает окно, в котором можно изменить название и описание текущей коллекции.  
**Удалить** - удаляет текущую коллекцию  
**Переименовать файлы** - переименует все файлы в коллекции в соответствии с маской (Пример маски: file-{0}, где вместо {0} будет подставлен номер).  
**Распределить по папкам** - распределяет коллекции по соответствующим папкам. **ВНИМАНИЕ! По умолчанию происходит перемещение файлов, для копирования при первом распределении нужно отметить соответствующий пункт при открытии папки (Файл -> Открыть папку)**
### Опции
**Очистить кеш изображений** - удаляет миниатюры изображений, созданные программой для более быстрого отображения в следующий раз  
**Тема** - раскрывает меню со списком доступных тем:
- Светлая
- Темная (от MadScorpion | Discord: MadScorpion#9411)
## Настройки открытия папки
**Базовая директория** - поле, в котором отображается выбранная для обработки в программе директория. Кнопка выбора находится справа от поля.  
**Рекурсивный поиск** - выполнить поиск не только в базовой директории, но и во всех ее поддиректориях. Выполняется один раз, при инициализации коллекции, в дальнейшем при открытии коллекций будет выполняться обычный поиск.  
**Маска для первого поиска** - поле для ввода пользовательской маски, которая будет использоваться только при первом поиске файлов.  
**Копировать все в отдельную папку** - при первом распределении копирует все коллекции с файлами в директорию, выбранную ниже (поле “Директория для размещения”).  
**Директория для размещения** - поле, в котором отображается директория для первого распределения. Кнопка выбора находится справа от поля и становится активной, если активен пункт “Копировать все в отдельную папку”
## Интеграция
Набор аргументов|Описание
---|---
"[ПАПКА]"|Открыть папку
"[ПАПКА]" -oc|Открыть коллекции