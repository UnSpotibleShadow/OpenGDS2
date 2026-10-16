# Consumer investigation conversations

The original result mode `009EFA40 +3A50..4186` presents consumer-representative
and secretary pages. The refund branches explain sold-plus-stock quantity,
per-copy recall price, total charge and the secretary's reaction/advice. The
no-recall branch gives a relieved secretary response. The original investigation
mode starts the five-week countdown before the later result.

The port now presents those roles and explanatory stages through its compact
conversation UI before the existing saved receipt. English and Japanese use
authored localized summaries of the recovered exchange; they are not verbatim
original dialogue imports. Secretary identity/portrait follows the hired secretary.
The consumer representative is named without assigning an unverified portrait.
The existing compact receipt retains Save, Load, Continue and funds feedback.

No refund, trigger, countdown, RNG or bankruptcy rule changes. At presentation
time inventory has already been withdrawn. Quantity is derived from the saved
refund divided by the applicable per-copy rate, only when that division is exact.
Zero-price or inconsistent legacy records show an explicit unknown quantity;
no stock is fabricated. Rates come from the result's existing retail-price or
fixed-1000 branch. Pending results prevent price editing in the supported flow.

The conversation precedes acknowledgement, so office time stays suspended.
Loading a pending event replays its explanatory pages, then shows the receipt;
it neither rerolls nor charges again. No new save fields or page cursor are
introduced. Continue on the receipt performs the existing acknowledgement.
The original native modal timing and exact global RNG stream are not certified.

Validation covers English/Japanese investigation, full-price/fixed-price/dismissed
results, substituted amounts, compact receipts, Save/Load replay, acknowledgement
and unchanged funds. Captures and executable traces remain private outside git.
