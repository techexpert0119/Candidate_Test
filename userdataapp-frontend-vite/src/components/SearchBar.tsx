import React, { useState } from "react";
import { TextField, Box } from "@mui/material";
import type { SearchOptions } from "../types/userData";
import { useDebounce } from "../hooks/useDebounce";

interface SearchBarProps {
  searchOptions: SearchOptions;
  onSearchChange: (options: SearchOptions) => void;
}

const SearchBar: React.FC<SearchBarProps> = ({
  searchOptions,
  onSearchChange,
}) => {
  const [localSearchOptions, setLocalSearchOptions] =
    useState<SearchOptions>(searchOptions);
  const debouncedSearchOptions = useDebounce(localSearchOptions, 500);

  // Update local state when props change
  React.useEffect(() => {
    setLocalSearchOptions(searchOptions);
  }, [searchOptions]);

  // Call onSearchChange when debounced value changes
  React.useEffect(() => {
    if (
      JSON.stringify(debouncedSearchOptions) !== JSON.stringify(searchOptions)
    ) {
      onSearchChange(debouncedSearchOptions);
    }
  }, [debouncedSearchOptions]);

  const handleChange =
    (field: keyof SearchOptions) =>
    (event: React.ChangeEvent<HTMLInputElement>) => {
      setLocalSearchOptions((prev) => ({
        ...prev,
        [field]: event.target.value || undefined,
      }));
    };

  return (
    <Box sx={{ mb: 3 }}>
      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 2 }}>
        <Box sx={{ flex: "1 1 200px", minWidth: "200px" }}>
          <TextField
            fullWidth
            size="small"
            label="First Name"
            value={localSearchOptions.firstName || ""}
            onChange={handleChange("firstName")}
          />
        </Box>
        <Box sx={{ flex: "1 1 200px", minWidth: "200px" }}>
          <TextField
            fullWidth
            size="small"
            label="Last Name"
            value={localSearchOptions.lastName || ""}
            onChange={handleChange("lastName")}
          />
        </Box>
        <Box sx={{ flex: "1 1 200px", minWidth: "200px" }}>
          <TextField
            fullWidth
            size="small"
            label="Email"
            value={localSearchOptions.email || ""}
            onChange={handleChange("email")}
          />
        </Box>
        <Box sx={{ flex: "1 1 200px", minWidth: "200px" }}>
          <TextField
            fullWidth
            size="small"
            label="Title"
            value={localSearchOptions.title || ""}
            onChange={handleChange("title")}
          />
        </Box>
        <Box sx={{ flex: "1 1 200px", minWidth: "200px" }}>
          <TextField
            fullWidth
            size="small"
            label="Comments"
            value={localSearchOptions.comments || ""}
            onChange={handleChange("comments")}
          />
        </Box>
      </Box>
    </Box>
  );
};

export default SearchBar;
