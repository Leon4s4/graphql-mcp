"""Setup script for graphql-mcp-python package."""

from setuptools import setup, find_packages

setup(
    name="graphql-mcp-python",
    use_scm_version=True,
    setup_requires=["setuptools_scm"],
    package_dir={"": "src"},
    packages=find_packages(where="src"),
    include_package_data=True,
    zip_safe=False,
)