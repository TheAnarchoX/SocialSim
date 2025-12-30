# GitHub Issues Creation Guide

This guide provides multiple ways to create GitHub issues from the ROADMAP.md.

## Option 1: Using the Bash Script (Recommended)

Run the provided script to create all issues with parent/sub-issue structure:

```bash
# Make the script executable
chmod +x scripts/create-roadmap-issues.sh

# Run the script
bash scripts/create-roadmap-issues.sh
```

**What the script creates:**
- **Parent issues** (labeled with 'epic') for each phase section
- **Sub-issues** for each major task that reference their parent issue
- Proper labels, dependencies, and milestones
- Links to relevant documentation

**Example structure:**
```
[Phase 1.1] Social Network Data Model Design (epic)
├── Design comprehensive ERD for social network
├── Design graph data model for Neo4J
├── Design relational schema for PostgreSQL
└── Document and validate data model
```

## Option 2: Manual Creation via GitHub CLI

If you prefer to create issues individually:

```bash
# Create a parent issue
gh issue create \
  --repo TheAnarchoX/SocialSim \
  --title "[Phase 1.1] Social Network Data Model Design" \
  --label "phase-1,data-model,critical,epic" \
  --body "See ROADMAP.md Phase 1.1 for details"

# Create a sub-issue
gh issue create \
  --repo TheAnarchoX/SocialSim \
  --title "Design comprehensive ERD for social network" \
  --label "phase-1,data-model,critical" \
  --body "Parent Issue: #1\n\nSee ROADMAP.md Phase 1.1"
```

## Option 3: GitHub Web Interface

1. Go to https://github.com/TheAnarchoX/SocialSim/issues/new
2. Create parent issue with [Phase X.Y] prefix and 'epic' label
3. Create sub-issues referencing the parent with #issue_number
4. Add appropriate labels manually

## Issue Structure

### Phase 1: Data Foundation
**Parent Issues (3):**
- [Phase 1.1] Social Network Data Model Design (4 sub-issues)
- [Phase 1.2] Simulation Data Model Design (4 sub-issues)
- [Phase 1.3] AT Protocol Data Model Extension (4 sub-issues)

### Phase 2: Storage & Persistence Layer
**Parent Issues (4):**
- [Phase 2.1] Neo4J Repository Layer & Custom Query Builder (5 sub-issues)
- [Phase 2.2] PostgreSQL Repository Layer (EF Core) (5 sub-issues)
- [Phase 2.3] Redis Integration (4 sub-issues)
- [Phase 2.4] Data Access Testing & Performance (3 sub-issues)

### Phases 3-7
Currently, the script covers Phases 1-2. You can extend it for remaining phases following the same pattern.

**Total Issues (Phases 1-2):**
- 7 parent issues (epic)
- 29 sub-issues
- **36 issues total** for Phases 1-2

## Labels Used

- **Phase labels**: `phase-1`, `phase-2`, etc.
- **Component labels**: `data-model`, `neo4j`, `postgresql`, `redis`, `at-protocol`, etc.
- **Priority labels**: `critical`, `high-priority`
- **Epic label**: `epic` (for parent issues)
- **Type labels**: `infrastructure`, `testing`, `performance`, `documentation`, etc.

## Viewing Issues

```bash
# View all epic (parent) issues
gh issue list --repo TheAnarchoX/SocialSim --label epic

# View issues for a specific phase
gh issue list --repo TheAnarchoX/SocialSim --label phase-1

# View all issues
gh issue list --repo TheAnarchoX/SocialSim
```

Or visit: https://github.com/TheAnarchoX/SocialSim/issues

## Notes

- Parent issues (epics) provide overview and track overall progress
- Sub-issues contain detailed tasks with checkboxes
- All issues reference ROADMAP.md for complete details
- Dependencies and blocking relationships noted in parent issues
- Documentation links included where relevant (e.g., AT_PROTOCOL_CUSTOM_HANDLE.md)

## Quick Start

```bash
# 1. Navigate to repository
cd /path/to/SocialSim

# 2. Authenticate with GitHub (if not already)
gh auth login

# 3. Run the script
bash scripts/create-roadmap-issues.sh
```

The script will output progress and issue numbers as it creates them.
