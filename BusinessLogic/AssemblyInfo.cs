// Atrybuty asemblii (pliku zawierającego skompilowany kod programu) i ustawienia dla całego projektu

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// umożliwia testom jednostkowym dostęp do internal metod i klas (normalnie internal są dostępne tylko wewnątrz tej samej biblioteki)
[assembly: InternalsVisibleTo("BusinessLogicTest")]
// wyłącza kompatybilność z technologią COM
[assembly: ComVisible(false)]
[assembly: Guid("4a734722-dd89-43c0-ae05-70f53f6dfe4b")]