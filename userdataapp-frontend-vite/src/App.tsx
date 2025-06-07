import { Container } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { useSearchParams } from "react-router-dom";
import SearchBar from "./components/SearchBar";
import FilterPanel from "./components/FilterPanel";
import UserDataTable from "./components/UserDataTable";
import { getAllUsers } from "./services/api";
import type { SearchOptions, FilterOptions } from "./types/userData";

function App() {
  const [searchParams, setSearchParams] = useSearchParams();

  const page = parseInt(searchParams.get("page") || "1");
  const rowsPerPage = parseInt(searchParams.get("pageSize") || "10");
  const searchOptions: SearchOptions = {
    firstName: searchParams.get("firstName") || undefined,
    lastName: searchParams.get("lastName") || undefined,
    email: searchParams.get("email") || undefined,
    title: searchParams.get("title") || undefined,
    comments: searchParams.get("comments") || undefined,
  };
  const filterOptions: FilterOptions = {
    gender: searchParams.get("gender") || undefined,
    country: searchParams.get("country") || undefined,
    registrationDateFrom: searchParams.get("registrationDateFrom") || undefined,
    registrationDateTo: searchParams.get("registrationDateTo") || undefined,
    birthDateFrom: searchParams.get("birthDateFrom") || undefined,
    birthDateTo: searchParams.get("birthDateTo") || undefined,
    minSalary: searchParams.get("minSalary")
      ? parseFloat(searchParams.get("minSalary")!)
      : undefined,
    maxSalary: searchParams.get("maxSalary")
      ? parseFloat(searchParams.get("maxSalary")!)
      : undefined,
  };

  const { data, isLoading } = useQuery({
    queryKey: ["users", page, rowsPerPage, searchOptions, filterOptions],
    queryFn: () => getAllUsers(page, rowsPerPage, searchOptions, filterOptions),
  });

  const updateSearchParams = (updates: Record<string, string | undefined>) => {
    const newParams = new URLSearchParams(searchParams);
    Object.entries(updates).forEach(([key, value]) => {
      if (value) {
        newParams.set(key, value);
      } else {
        newParams.delete(key);
      }
    });
    setSearchParams(newParams);
  };

  const handleSearchChange = (options: SearchOptions) => {
    const updates: Record<string, string | undefined> = {
      page: "1", // Reset to first page on search
    };

    Object.entries(options).forEach(([key, value]) => {
      updates[key] = value || undefined;
    });

    updateSearchParams(updates);
  };

  const handleFilterChange = (options: FilterOptions) => {
    const updates: Record<string, string | undefined> = {
      page: "1", // Reset to first page on filter
    };

    Object.entries(options).forEach(([key, value]) => {
      updates[key] = value?.toString() || undefined;
    });

    updateSearchParams(updates);
  };

  const handlePageChange = (newPage: number) => {
    updateSearchParams({ page: newPage.toString() });
  };

  const handleRowsPerPageChange = (newRowsPerPage: number) => {
    updateSearchParams({
      pageSize: newRowsPerPage.toString(),
      page: "1", // Reset to first page when changing rows per page
    });
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <SearchBar
        searchOptions={searchOptions}
        onSearchChange={handleSearchChange}
      />
      <FilterPanel
        filters={filterOptions}
        onFilterChange={handleFilterChange}
      />
      <UserDataTable
        users={data?.data || []}
        isLoading={isLoading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={data?.totalCount || 0}
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
      />
    </Container>
  );
}

export default App;
