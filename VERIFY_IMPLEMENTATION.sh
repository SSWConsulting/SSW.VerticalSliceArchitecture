#!/bin/bash

echo "╔══════════════════════════════════════════════════════════════════════════════╗"
echo "║              FastEndpoints Implementation Verification                       ║"
echo "╚══════════════════════════════════════════════════════
echo ""

# Color codes
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

verify_file() {
    if [ -f "$1" ]; then
        echo -e "${GREEN}✓${NC} $1"
        return 0
    else
        echo -e "✗ $1 (MISSING)"
        return 1
    fi
}

verify_dir() {
    if [ -d "$1" ]; then
        echo -e "${GREEN}✓${NC} $1/"
        return 0
    else
        echo -e "✗ $1/ (MISSING)"
        return 1
    fi
}

echo "=== Package References ==="
grep -q "FastEndpoints" /work/Directory.Packages.props && echo -e "${GREEN}✓${NC} FastEndpoints in Directory.Packages.props" || echo "✗ FastEndpoints in Directory.Packages.props"
grep -q "FastEndpoints" /work/src/WebApi/WebApi.csproj && echo -e "${GREEN}✓${NC} FastEndpoints in WebApi.csproj" || echo "✗ FastEndpoints in WebApi.csproj"
echo ""

echo "=== Infrastructure Directory ==="
verify_dir "/work/src/WebApi/Common/FastEndpoints"
echo ""

echo "=== Infrastructure Files ==="
verify_file "/work/src/WebApi/Common/FastEndpoints/EndpointBase.cs"
verify_file "/work/src/WebApi/Common/FastEndpoints/FastEndpointEventPublisher.cs"
verify_file "/work/src/WebApi/Common/FastEndpoints/LoggingPreProcessor.cs"
verify_file "/work/src/WebApi/Common/FastEndpoints/PerformancePreProcessor.cs"
verify_file "/work/src/WebApi/Common/FastEndpoints/PerformancePostProcessor.cs"
verify_file "/work/src/WebApi/Common/FastEndpoints/UnhandledExceptionHandler.cs"
echo ""

echo "=== Heroes FastEndpoints ==="
verify_file "/work/src/WebApi/Features/Heroes/HeroesGroup.cs"
verify_file "/work/src/WebApi/Features/Heroes/Commands/CreateHeroFastEndpoint.cs"
verify_file "/work/src/WebApi/Features/Heroes/Commands/UpdateHeroFastEndpoint.cs"
verify_file "/work/src/WebApi/Features/Heroes/Queries/GetAllHeroesFastEndpoint.cs"
echo ""

echo "=== Event Handlers ==="
verify_file "/work/src/WebApi/Features/Teams/Events/PowerLevelUpdatedFastEventHandler.cs"
echo ""

echo "=== Modified Files ==="
grep -q "UseFastEndpoints" /work/src/WebApi/Program.cs && echo -e "${GREEN}✓${NC} Program.cs updated" || echo "✗ Program.cs not updated"
grep -q "AddFastEndpoints" /work/src/WebApi/Host/DependencyInjection.cs && echo -e "${GREEN}✓${NC} DependencyInjection.cs updated" || echo "✗ DependencyInjection.cs not updated"
grep -q "Mode.WaitForAll" /work/src/WebApi/Common/Middleware/EventualConsistencyMiddleware.cs && echo -e "${GREEN}✓${NC} EventualConsistencyMiddleware.cs updated" || echo "✗ EventualConsistencyMiddleware.cs not updated"
echo ""

echo "=== Documentation Files ==="
verify_file "/work/FASTENDPOINTS_IMPLEMENTATION.md"
verify_file "/work/IMPLEMENTATION_CHECKLIST.md"
verify_file "/work/CHANGES_SUMMARY.md"
verify_file "/work/QUICKSTART.md"
verify_file "/work/docs/FASTENDPOINTS_COMPARISON.md"
verify_file "/work/src/WebApi/Features/Heroes/README.FastEndpoints.md"
echo ""

echo "=== Statistics ==="
echo -e "${YELLOW}New Files Created:${NC}"
find /work/src/WebApi/Common/FastEndpoints -type f -name "*.cs" 2>/dev/null | wc -l | xargs echo "  Infrastructure files:"
find /work/src/WebApi/Features/Heroes -type f -name "*Fast*.cs" 2>/dev/null | wc -l | xargs echo "  Heroes endpoints:"
find /work/src/WebApi/Features/Teams/Events -type f -name "*Fast*.cs" 2>/dev/null | wc -l | xargs echo "  Event handlers:"
find /work -maxdepth 2 -name "*.md" -exec grep -l "FastEndpoints" {} \; 2>/dev/null | wc -l | xargs echo "  Documentation files:"
echo ""

echo -e "${YELLOW}Total Lines of Code:${NC}"
find /work/src/WebApi -name "*Fast*.cs" -type f 2>/dev/null -exec cat {} \; | wc -l | xargs echo "  FastEndpoints code:"
echo ""

"echo "╔═
echo "║                          Verification Complete                               ║"
echo "╚══════════════════════════════════════════════════════════════════════════════╝"
echo ""
echo "Next steps:"
echo "  1. Run: dotnet build"
echo "  2. Run: dotnet test"
echo "  3. Read: QUICKSTART.md"
echo "  4. Review: FASTENDPOINTS_IMPLEMENTATION.md"
echo ""
