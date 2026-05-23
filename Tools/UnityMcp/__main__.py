"""Entry point: python -m Tools.UnityMcp"""
import asyncio
from .server import main
asyncio.run(main())
