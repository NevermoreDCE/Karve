import { create } from "zustand";
import type { CompanyDto } from "../types/api";

interface TenantState {
  availableCompanies: CompanyDto[];
  selectedCompanyId: string | null;
  setAvailableCompanies: (companies: CompanyDto[]) => void;
  setSelectedCompanyId: (companyId: string) => void;
  clearTenantContext: () => void;
}

export const useTenantStore = create<TenantState>((set, get) => ({
  availableCompanies: [],
  selectedCompanyId: null,

  setAvailableCompanies: (companies) => {
    const currentSelected = get().selectedCompanyId;
    const containsCurrent =
      currentSelected !== null && companies.some((c) => c.id === currentSelected);

    set({
      availableCompanies: companies,
      selectedCompanyId: containsCurrent
        ? currentSelected
        : companies.length > 0
        ? companies[0].id
        : null,
    });
  },

  setSelectedCompanyId: (companyId) => set({ selectedCompanyId: companyId }),

  clearTenantContext: () =>
    set({
      availableCompanies: [],
      selectedCompanyId: null,
    }),
}));
