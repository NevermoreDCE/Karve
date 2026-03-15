import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { useMsal } from "@azure/msal-react";
import { getCompanies } from "../api/companiesApi";
import { useTenantStore } from "../state/tenantStore";
import type { CompanyDto } from "../types/api";

interface CurrentUserProfile {
  externalUserId: string | null;
  email: string | null;
  displayName: string | null;
}

interface CurrentUserResult {
  profile: CurrentUserProfile;
  memberships: CompanyDto[];
  selectedCompanyId: string | null;
  setSelectedCompanyId: (companyId: string) => void;
  isLoading: boolean;
  error: string | null;
}

function readClaim(
  claims: Record<string, unknown> | undefined,
  name: string
): string | null {
  const value = claims?.[name];
  return typeof value === "string" ? value : null;
}

export function useCurrentUser(): CurrentUserResult {
  const { accounts } = useMsal();
  const account = accounts[0];
  const claims = (account?.idTokenClaims ?? {}) as Record<string, unknown>;

  const memberships = useTenantStore((state) => state.availableCompanies);
  const selectedCompanyId = useTenantStore((state) => state.selectedCompanyId);
  const setAvailableCompanies = useTenantStore((state) => state.setAvailableCompanies);
  const setSelectedCompanyId = useTenantStore((state) => state.setSelectedCompanyId);

  const companiesQuery = useQuery({
    queryKey: ["current-user", "memberships"],
    queryFn: getCompanies,
    enabled: !!account,
  });

  useEffect(() => {
    if (companiesQuery.data) {
      setAvailableCompanies(companiesQuery.data);
    }
  }, [companiesQuery.data, setAvailableCompanies]);

  const profile: CurrentUserProfile = {
    externalUserId:
      readClaim(claims, "oid") ?? readClaim(claims, "sub") ?? account?.homeAccountId ?? null,
    email:
      readClaim(claims, "preferred_username") ??
      readClaim(claims, "email") ??
      account?.username ??
      null,
    displayName:
      readClaim(claims, "name") ??
      (typeof account?.name === "string" ? account.name : null),
  };

  return {
    profile,
    memberships,
    selectedCompanyId,
    setSelectedCompanyId,
    isLoading: companiesQuery.isLoading,
    error: companiesQuery.isError ? companiesQuery.error.message : null,
  };
}
