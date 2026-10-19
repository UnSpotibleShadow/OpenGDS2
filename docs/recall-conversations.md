# Consumer investigation conversations

The original fGMKikakuKaigi procedure `009EFA40` has three investigation pages
at `+38C7..39D3`: secretary warning, consumer representative introduction and
representative warning that results are due in a month and a full recall may be
required at the company's expense. The dialogue says a month; the recovered
countdown remains five weekly boundaries. Presentation does not alter that rule.

Mode 12 starts with a secretary result introduction at `+3A12..3A43`. Full-price
and fixed-¥1,000 branches then have seven pages: representative greeting,
recall order, secretary reaction, sold-plus-stock quantity/rate, total cost,
a separate dismayed reaction and advice. Full-price and fixed-price reactions
are distinct. The dismissed branch has just the introduction and relief page.
These yield three, eight and two pages, respectively. No separate receipt or
Continue screen follows the original dialogue.

Dialogue schema 19 imports fifteen bindings from constant pool `005FAFCC`,
indices `ED..F3`, `F5..F8`, `FA`, `FD..105`, into the player's local cache.
Dynamic title, quantity, price and refund values remain placeholders. Original
Japanese bytes are not committed. English uses faithful translations; older
Japanese caches use authored localized fallbacks until reimported. Secretary
identity/portrait follows the hired secretary. The representative still has no
portrait assigned without separately verified portrait-argument semantics.

The final page now acknowledges the existing pending recall and returns to the
existing sales route. Save/Load remain available on the desktop toolbar. Loading
an unread event replays its pages, with no new refund or draw. Old pending events
use the same path; no recall save fields or page-cursor migration are introduced.
Quantity still comes from the saved refund divided by its applicable per-copy
rate because stock was already withdrawn. Zero-price/inconsistent legacy values
remain explicitly unknown. Refund formulas, scheduling, countdown, accounting,
RNG and bankruptcy rules are unchanged by this presentation correction.

Verification covers all four outcomes and investigation, English/Japanese page
counts, faithful substitutions, the unknown-quantity fallback, pending save/load,
direct final-page acknowledgement and no receipt or repeated charge. Optional
private-original domain checks validate schema/count and native text bindings.
Native Windows modal timing, exact pixels and full global RNG parity remain
unverified.
