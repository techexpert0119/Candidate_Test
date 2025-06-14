import React, { useState, useEffect, useCallback, useMemo } from "react";
import { Box, TextField, MenuItem, Slider, Typography } from "@mui/material";
import { DatePicker } from "@mui/x-date-pickers";
import { useDebounce } from "../hooks/useDebounce";
import type { FilterOptions } from "../types/userData";
import Select from "react-select";
import type { SingleValue } from "react-select";
import countryList from "react-select-country-list";

interface FilterPanelProps {
  filters: FilterOptions;
  onFilterChange: (filters: FilterOptions) => void;
}

const FilterPanel: React.FC<FilterPanelProps> = ({
  filters,
  onFilterChange,
}) => {
  const [localFilters, setLocalFilters] = useState<FilterOptions>(filters);
  const countryOptions = useMemo(() => countryList().getData(), []);
  const debouncedFilters = useDebounce(localFilters, 500);

  useEffect(() => {
    setLocalFilters(filters);
  }, [filters]);

  useEffect(() => {
    if (JSON.stringify(debouncedFilters) !== JSON.stringify(filters)) {
      onFilterChange(debouncedFilters);
    }
  }, [debouncedFilters]);

  const handleChange = useCallback(
    (field: keyof FilterOptions) =>
      (event: React.ChangeEvent<HTMLInputElement | { value: unknown }>) => {
        setLocalFilters((prev) => ({
          ...prev,
          [field]: event.target.value || undefined,
        }));
      },
    []
  );

  const handleCountryChange = useCallback(
    (selectedOption: SingleValue<{ value: string; label: string }>) => {
      setLocalFilters((prev) => ({
        ...prev,
        country: selectedOption?.label || undefined,
      }));
    },
    []
  );

  const handleDateChange = useCallback(
    (field: keyof FilterOptions) => (date: Date | null) => {
      setLocalFilters((prev) => ({
        ...prev,
        [field]: date?.toISOString() || null,
      }));
    },
    []
  );

  const handleSalaryChange = useCallback(
    (_event: Event, newValue: number | number[]) => {
      const [min, max] = newValue as number[];
      setLocalFilters((prev) => ({
        ...prev,
        minSalary: min,
        maxSalary: max,
      }));
    },
    []
  );

  return (
    <Box sx={{ mb: 2 }}>
      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 2 }}>
        <Box
          sx={{
            width: { xs: "100%", sm: "calc(50% - 8px)", md: "calc(20% - 8px)" },
          }}
        >
          <DatePicker
            label="Registration From"
            value={
              localFilters.registrationDateFrom
                ? new Date(localFilters.registrationDateFrom)
                : null
            }
            onChange={handleDateChange("registrationDateFrom")}
            slotProps={{ textField: { size: "small", fullWidth: true } }}
          />
        </Box>
        <Box
          sx={{
            width: { xs: "100%", sm: "calc(50% - 8px)", md: "calc(20% - 8px)" },
          }}
        >
          <DatePicker
            label="Registration To"
            value={
              localFilters.registrationDateTo
                ? new Date(localFilters.registrationDateTo)
                : null
            }
            onChange={handleDateChange("registrationDateTo")}
            slotProps={{ textField: { size: "small", fullWidth: true } }}
          />
        </Box>
        <Box
          sx={{
            width: { xs: "100%", sm: "calc(50% - 8px)", md: "calc(20% - 8px)" },
          }}
        >
          <DatePicker
            label="Birth Date From"
            value={
              localFilters.birthDateFrom
                ? new Date(localFilters.birthDateFrom)
                : null
            }
            onChange={handleDateChange("birthDateFrom")}
            slotProps={{ textField: { size: "small", fullWidth: true } }}
          />
        </Box>
        <Box
          sx={{
            width: { xs: "100%", sm: "calc(50% - 8px)", md: "calc(20% - 8px)" },
          }}
        >
          <DatePicker
            label="Birth Date To"
            value={
              localFilters.birthDateTo
                ? new Date(localFilters.birthDateTo)
                : null
            }
            onChange={handleDateChange("birthDateTo")}
            slotProps={{ textField: { size: "small", fullWidth: true } }}
          />
        </Box>
        <Box
          sx={{
            width: { xs: "100%", sm: "calc(50% - 8px)", md: "calc(20% - 8px)" },
          }}
        >
          <TextField
            select
            fullWidth
            size="small"
            label="Gender"
            value={localFilters.gender || ""}
            onChange={handleChange("gender")}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem key={"Male"} value={"Male"}>
              Male
            </MenuItem>
            <MenuItem key={"Female"} value={"Female"}>
              Female
            </MenuItem>
          </TextField>
        </Box>
        <Box
          sx={{
            width: { xs: "100%", sm: "calc(50% - 8px)", md: "calc(20% - 8px)" },
          }}
        >
          <Select
            options={countryOptions}
            value={
              countryOptions.find(
                (option) => option.label === localFilters.country
              ) || null
            }
            onChange={handleCountryChange}
            isClearable
            styles={{
              menu: (base) => ({
                ...base,
                zIndex: 9999,
              }),
            }}
          />
        </Box>
      </Box>
      <Box sx={{ mt: 2 }}>
        <Typography gutterBottom>Salary Range</Typography>
        <Slider
          value={[
            localFilters.minSalary || 0,
            localFilters.maxSalary || 300000,
          ]}
          onChange={handleSalaryChange}
          valueLabelDisplay="auto"
          min={0}
          max={300000}
          step={1000}
        />
      </Box>
    </Box>
  );
};

export default FilterPanel;
