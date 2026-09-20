# plans/

How this repository is built, and the record of it.

Work starts from a GitHub issue. The issue says what is needed and why, and leaves the mechanism
open. A plan file here turns one issue, or one track spanning several, into ordered units of work.

Each plan follows a fixed skeleton: `At a glance`, `Progress`, `Context`, `Design decisions`,
`Steps`, and optionally `Out of scope` and `Open questions`. The Progress table sits directly under
the TL;DR so a session that starts cold can read two sections and know where to resume. Units are
labelled `Step N`, or phase-prefixed (`A1`, `B2`) once there are more than about six.

A step is marked in progress before any code is touched, and done only after its commit lands. One
step is one commit. The plan is updated first, then the code, so the plan never trails reality.

`plan-bike-repair-shop.md` is the entry point for the application as a whole: the design decisions,
the scope boundary, the open questions and a phase-level progress table. Each phase has its own
`plan-<letter>-<name>.md` with the step table and the step descriptions, tied to one GitHub issue.
Supporting material that outgrows an inline section lives under `reference/` with its own index and a
header that says what it covers, so a session skips it unless it is working in that area.

`ROADMAP.md` is the cross-plan entry point: what is in flight, what is next, what is blocked. It is
a status view, not a session log.

Plans are committed deliberately. Together with `CLAUDE.md` they document how the solution was
designed and built, including the parts that were considered and dropped.
