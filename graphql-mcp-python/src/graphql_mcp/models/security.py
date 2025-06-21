"""Security-related types and models."""

from datetime import datetime
from typing import Any, Dict, List, Optional
from pydantic import Field

from .base import BaseModel, ValidationSeverity


class SecurityVulnerability(BaseModel):
    """Security vulnerability information."""
    id: str = ""
    title: str = ""
    description: str = ""
    severity: ValidationSeverity = ValidationSeverity.INFO
    category: str = ""
    affected_fields: List[str] = Field(default_factory=list)
    mitigation: str = ""
    references: List[str] = Field(default_factory=list)
    discovered_at: datetime = Field(default_factory=datetime.utcnow)


class SecurityInfo(BaseModel):
    """Security information for GraphQL operations."""
    requires_authentication: bool = False
    required_permissions: List[str] = Field(default_factory=list)
    sensitive_data_fields: List[str] = Field(default_factory=list)
    rate_limit_info: Optional[Dict[str, Any]] = None
    allowed_origins: List[str] = Field(default_factory=list)
    security_headers: Dict[str, str] = Field(default_factory=dict)


class SecurityAnalysis(BaseModel):
    """Security analysis results."""
    security_warnings: List[str] = Field(default_factory=list)
    required_permissions: List[str] = Field(default_factory=list)
    has_sensitive_data: bool = False
    security_recommendations: List[str] = Field(default_factory=list)
    vulnerability_score: int = 0
    compliance_status: str = "Unknown"


class SecurityAnalysisResult(BaseModel):
    """Comprehensive security analysis results."""
    endpoint_name: str = ""
    overall_security_score: int = 0
    vulnerabilities: List[SecurityVulnerability] = Field(default_factory=list)
    security_posture: str = "Unknown"
    compliance_checks: Dict[str, bool] = Field(default_factory=dict)
    recommendations: List[str] = Field(default_factory=list)
    risk_assessment: str = "Unknown"
    last_assessed: datetime = Field(default_factory=datetime.utcnow)


class SecurityComplianceResult(BaseModel):
    """Security compliance assessment."""
    standard: str = ""
    compliance_level: str = "Unknown"
    passed_checks: List[str] = Field(default_factory=list)
    failed_checks: List[str] = Field(default_factory=list)
    remediation_steps: List[str] = Field(default_factory=list)
    compliance_score: int = 0
    assessment_date: datetime = Field(default_factory=datetime.utcnow)


class PenetrationTestResult(BaseModel):
    """Penetration testing results."""
    test_name: str = ""
    test_category: str = ""
    result: str = "Unknown"
    vulnerability_found: bool = False
    severity: ValidationSeverity = ValidationSeverity.INFO
    description: str = ""
    proof_of_concept: Optional[str] = None
    remediation: str = ""
    tested_at: datetime = Field(default_factory=datetime.utcnow)


class RateLimitInfo(BaseModel):
    """Rate limiting information."""
    requests_per_minute: int = 0
    requests_per_hour: int = 0
    burst_limit: int = 0
    current_usage: int = 0
    reset_time: Optional[datetime] = None
    rate_limit_headers: Dict[str, str] = Field(default_factory=dict)