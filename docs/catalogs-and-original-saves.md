# Planning catalogs and original saves

Classification: verified static structures, not original runtime comparison.

## Genres

The initializer contains 18 calls to thunk `0060EE4C`. Each record occupies
4C bytes beginning at initializer +6331. Setter starts at `00981EA8`, size 164;
it writes seven arguments and increments member 11A0. Names identify these as
planning genres; the name column is member 11B4 (global `A121D0`).

| Setter argument | Member | Recovered type | Status |
| --- | --- | --- | --- |
| 10 | 1240 | Integer | Semantic role unresolved |
| 14 | 125C | Integer | Semantic role unresolved |
| 18 | 11B4 | String | Genre name |
| 1C | 11D0 | Integer | Semantic role unresolved |
| 20 | 11EC | Integer | Semantic role unresolved |
| 24 | 1208 | Integer | Semantic role unresolved |
| 28 | 1224 | Integer | Semantic role unresolved |

Python and C# extraction agree on every name, numeric column and call address.
The importer saves `catalog.json` only in the private user cache. No original
catalog records or descriptions are bundled. Monetary-looking integers are not
called license fees until selection/transaction data flow establishes that role.

The next initialized groups are audience records (helper `00981DCC`), technology
records (`0098BA28`) and advertising (`0097CA20`). Their initializer positions and
names were located; their rules are not yet implemented.

## Original .GD2 saves

Save form handler calls thunk `0060ECCC`, whose Module1 body begins `009D9824`,
size 6,952. At +0085 it opens a sequential output file; repeated `WriteFile`
opcodes serialize scalars/array records and +1AAA closes it. The load counterpart
begins `009D60E0`, size 6,476; +0016 opens sequential input and uses matching
`InputFile` operations. This is evidence for VB6 sequential records, not a
raw in-memory struct dump or the modern JSON format.

The initial records include a header variable, members 0000/0010, then members
0020/0030/0040/0050/0060. Known latter roles are funds, company name, year, month,
week. Further loops serialize catalogs, employees and workspace state. Stack
push order is reversed relative to source argument order, so a future format
map must correlate save and load before assigning record positions.

No original .GD2 fixture was supplied or generated. Header meaning, exact text
encoding, delimiters, array-loop cardinalities and error behavior remain unverified.
The reimplementation therefore does not import or export original saves yet.
A read-only inspector should precede any compatibility importer; never treat a
partially parsed original file as a complete company state.
