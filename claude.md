# Unity Multiplayer Platformer - Claude Code Sub-Agent Coordination

## Project Context
**Project**: Unity Multiplayer Platformer with Photon PUN 2
**Architecture**: Component-based with State Machine patterns
**Current Status**: Production-ready core systems with ongoing enhancements

## Sub-Agent Team Overview

You have access to four specialized sub-agents. **Use parallel delegation whenever possible** to maximize efficiency. Each agent has specific tools and expertise:

### 🔴 @AssetAuditor (Red)
**Use when you need**: Codebase analysis, performance assessment, technical feasibility evaluation
**Specializes in**: System architecture review, dependency mapping, technical debt identification
**Tools**: Grep, Glob, Read, LS (analysis-focused)
**Best for**: 
- "Analyze the current combat system for enhancement opportunities"
- "Assess the performance impact of adding new features"
- "Identify technical constraints for the proposed architecture"

### 🔵 @DesignConsultant (Blue)  
**Use when you need**: System design, feature specifications, architecture planning
**Specializes in**: Technical planning, UX/UI design, implementation roadmaps
**Tools**: WebSearch, Read, Write (research and planning)
**Best for**:
- "Design a new ability system architecture"
- "Create specifications for multiplayer combat enhancements"
- "Plan the implementation approach for new features"

### 🟢 @CodeArtisan (Green)
**Use when you need**: Implementation, code changes, bug fixes, system integration
**Specializes in**: Hands-on development, testing, performance optimization
**Tools**: Edit, MultiEdit, Bash, Write (execution-focused)
**Best for**:
- "Implement the designed ability system"
- "Fix the networking synchronization issue"
- "Integrate the new combat features with existing systems"

### 🟣 @ArchiveKeeper (Purple)
**Use when you need**: Documentation creation, knowledge organization, API references
**Specializes in**: Technical writing, documentation structure, developer guides
**Tools**: Write, Read, WebSearch (documentation-focused)
**Best for**:
- "Document the new combat system implementation"
- "Create developer guide for the ability system"
- "Update architecture documentation with recent changes"

## Delegation Patterns

### 1. **Parallel Analysis** (Use Often)
For complex tasks, delegate analysis and design simultaneously:
```
@AssetAuditor: Analyze current system constraints and performance
@DesignConsultant: Research best practices and design approaches
```

### 2. **Sequential Implementation**
Follow the natural workflow:
Analysis → Design → Implementation → Documentation

### 3. **Iterative Development** 
For large features, use multiple rounds:
- Analysis + Design → Implementation (Phase 1) → Review → Implementation (Phase 2) → Documentation

## Project-Specific Guidelines

### Unity Development Standards
- **Performance**: Maintain 60+ FPS, minimize GC allocation
- **Networking**: Use Photon PUN 2 patterns, Master Client authority
- **Architecture**: Component-based, state machine patterns
- **Code Style**: Follow existing patterns, comprehensive null checking

### File Organization
- **Core Scripts**: `/Assets/_Project/Scripts/`
- **Characters**: `/Assets/_Project/Scripts/Characters/`
- **State Machine**: `/Assets/_Project/Scripts/State Machine/`
- **Documentation**: Root level `.md` files

### Testing Approach
- Always run existing tests before major changes
- Use Unity Test Runner for unit testing
- Test both offline and multiplayer scenarios
- Performance test with Unity Profiler

## Context Management

### Long Sessions (>150 interactions)
- Use `/compact` to summarize progress and free context space
- Prioritize essential information in summaries
- Maintain agent delegation history

### Permission Management
- **Analysis tools** (Grep, Read, LS): Auto-approve recommended
- **Write operations**: Require confirmation for critical files
- **Bash commands**: Review build and test commands before execution

### Quality Assurance
Each sub-agent should:
- Validate their work against project standards
- Cross-reference with other agents' findings
- Document any assumptions or limitations
- Provide specific file references and line numbers

## Workflow Optimization

### For New Features
1. **@AssetAuditor** + **@DesignConsultant**: Parallel analysis and initial design
2. **@DesignConsultant**: Detailed specifications based on constraints
3. **@CodeArtisan**: Implementation in iterative phases
4. **@ArchiveKeeper**: Documentation and knowledge capture

### For Bug Fixes
1. **@AssetAuditor**: Root cause analysis and impact assessment
2. **@CodeArtisan**: Implementation with testing
3. **@ArchiveKeeper**: Update troubleshooting documentation

### For System Refactoring
1. **@AssetAuditor**: Current system analysis and improvement opportunities
2. **@DesignConsultant**: Refactoring plan with migration strategy
3. **@CodeArtisan**: Incremental implementation with validation
4. **@ArchiveKeeper**: Updated architecture and migration documentation

## Success Metrics
- **Parallel Efficiency**: Use multiple agents simultaneously when possible
- **Quality Consistency**: Each agent maintains >90% success rate with their tools
- **Context Optimization**: Sessions lasting 200+ interactions without degradation
- **Autonomous Operation**: Minimal human intervention required for standard workflows

## Emergency Protocols
- If any agent encounters blocking issues, immediately inform the user
- For critical system changes, always validate with affected systems first
- When in doubt about delegation, ask for clarification rather than assume

## Current Project Status
- **Active Sprint**: Enhanced Combat System Implementation
- **Next Priorities**: UI system enhancement, audio integration, animation polish
- **Long-term Goals**: Advanced AI, power-up systems, mobile optimization

Remember: **Think in parallel, delegate efficiently, and maintain quality standards throughout all operations.**