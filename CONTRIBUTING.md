# Contributing to Domain Copilot

## Development Workflow

1. Create a feature branch: `git checkout -b feature/your-feature-name`
2. Make your changes with atomic, well-described commits (Conventional Commits format)
3. Push your branch and open a Pull Request against `master`
4. Self-review your changes (inline comments on the diff)
5. Merge after CI checks pass

## Commit Message Convention

We use [Conventional Commits](https://www.conventionalcommits.org/):
- `feat:` new feature
- `fix:` bug fix
- `docs:` documentation only
- `test:` adding or fixing tests
- `chore:` maintenance tasks (dependencies, config)
- `ci:` CI/CD pipeline changes

## Running Tests

```powershell
dotnet test
```

See `README.md` for full setup instructions.