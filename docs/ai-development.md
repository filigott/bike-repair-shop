# Building this with an AI assistant

This repository was written by a developer working with Claude Code. The application is the
deliverable; this note describes the method, because the method is the part that transfers.

## The setup

Three things exist before any code does.

`CLAUDE.md` at the repository root holds the conventions both halves follow: the layer boundaries,
how a feature slice is shaped, what a DTO is called, which exception type belongs to which layer,
how tests are structured. It is loaded into every session automatically. An assistant that has read
it produces code that looks like the code already here, rather than code that merely works.

`plans/` holds the design record. One plan per phase, each with a fixed skeleton: what it is, a
progress table, the context a cold session needs, the design decisions with their reasoning, and the
ordered steps. `plans/ROADMAP.md` sits above them as the cross-plan view of what is in flight and
what is next. The plans are committed on purpose. They are how a reader finds out not only what was
built but what was weighed and rejected.

GitHub issues carry the what and the why, and deliberately stop short of the how. An issue that
prescribes a mechanism removes the implementor's judgement, and an assistant treats a confident
"Proposal" section as a decision already taken.

## The loop

Work runs in one reviewable unit at a time.

1. An issue exists before the work does.
2. A plan turns the issue into ordered steps, and the step is marked in progress before any code is
   touched, so the plan never trails reality.
3. The assistant builds exactly one step, with its tests, and stops.
4. It presents what changed one line per file, the state of the gates, and a commit message.
5. The developer decides. Nothing is committed, pushed or merged without a word for that specific
   chunk.

Every unit ends the same way: build, lint, typecheck, the full test suites. A new test is shown
failing against the exact line it pins before it counts as coverage, because a test written
alongside the code it tests passes on the first run for the wrong reasons more often than not.

Code reaches `main` only through a pull request. A commit that touches nothing but `plans/` goes
straight to `main`, because it changes no behaviour and a pull request would only cost a CI run.

## What stays with the human

The assistant does not run the manual pass. Work ends with a person clicking through the running
application, after every phase at first and batched across a few phases later on, and those passes have been the highest-yield step in the whole process. They
have surfaced things no test was going to: a detail page that showed three forms at once when it
should have offered one action at a time, a back link nobody could see, a page pinned to the left
edge of the screen while its dialogs centred themselves, an order with no human-readable number.

The assistant also does not decide. Where a choice has more than one defensible answer it goes back
as an explicit question with options and a recommendation, and the developer picks. The record of
those choices is in the plan files, which is why the reasoning survives the session.

## What it is good at, and what it is not

It is good at consistency at volume. Every handler has the same shape, every DTO is named the same
way, both locale files change in the same commit, and the conventions hold across a hundred commits
without drifting.

It is not reliably good at knowing when to stop. Left alone it adds structure ahead of any consumer.
Roughly the first half of the elapsed time here bought a foundation and none of the four things the
brief actually asks for. The correction was explicit: a new design decision needs a consumer in the
current or next phase, or it waits.

It also asserts rather than verifies unless told not to. Two examples from this repository. Values
read out of a form were cast to their enum type, which told the compiler something nobody had
checked, so a malformed request failed as "undefined is not a function" instead of naming the
problem. A field was projected onto every API response for a feature that, when finally built, could
not use it. Both were caught by a human reading the code and asking the obvious question.

The most valuable single intervention was a rejection. Three options were put up for where a mapping
between UI commands and order statuses should live. All three were rejected in one sentence, because
every one of them moved the problem rather than removed it. The real fault was the data's shape. The
state machine was keyed by target status, which cannot tell two edges apart when they share one.
Rekeying it by the action deleted the mapping outright and fixed two defects the old shape could not
express. The rule that leaves behind is worth more than the code: when every option on the table
relocates a problem, none of them is the answer, and the thing to question is what the data is
keyed by.

## What it cost

About eleven and a half hours of focused work across five days produced the application, its packaging, its
test suites and this record. The measured breakdown is in `plans/reference/time-and-scope.md`, which
tracks the cost honestly, including the stretches that bought foundation rather than features.

About half the commits touch only plans, conventions or documentation. That ratio is the
method, not overhead: the plans are what let a cold session resume without re-deriving the design,
and they are why the reasoning behind a decision is still available months later.
