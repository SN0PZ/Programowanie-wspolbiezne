// Konfiguracja asemblii tak, aby była niewidoczna dla COM,
// ale umożliwiała testowanie internal kodu w projekcie DataTest

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("DataTest")]
[assembly: ComVisible(false)]
[assembly: Guid("bed985f7-aa6b-4c6f-b764-ad81e70f2dd2")]