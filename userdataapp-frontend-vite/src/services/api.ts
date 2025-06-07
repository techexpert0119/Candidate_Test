import axios from "axios";
import type {
  UserData,
  FilterOptions,
  SearchOptions,
  PaginatedResponse,
} from "../types/userData";

const API_BASE_URL = "http://localhost:5220/api";

export const getAllUsers = async (
  page: number,
  pageSize: number,
  searchOptions: SearchOptions,
  filterOptions: FilterOptions
): Promise<PaginatedResponse<UserData>> => {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });

  // Add search parameters
  Object.entries(searchOptions).forEach(([key, value]) => {
    if (value) {
      params.append(key, value);
    }
  });

  // Add filter parameters
  Object.entries(filterOptions).forEach(([key, value]) => {
    if (value !== null && value !== undefined) {
      if (value instanceof Date) {
        params.append(key, value.toISOString());
      } else {
        params.append(key, value.toString());
      }
    }
  });

  const response = await fetch(`${API_BASE_URL}/UserData?${params.toString()}`);
  if (!response.ok) {
    throw new Error("Failed to fetch users");
  }
  return response.json();
};

export const userDataApi = {
  async getAllUsers(
    page = 1,
    pageSize = 10
  ): Promise<PaginatedResponse<UserData>> {
    const response = await axios.get<PaginatedResponse<UserData>>(
      `${API_BASE_URL}/UserData`,
      {
        params: { page, pageSize },
      }
    );
    return response.data;
  },

  async getFilteredUsers(
    filters: FilterOptions,
    page = 1,
    pageSize = 10
  ): Promise<PaginatedResponse<UserData>> {
    const response = await axios.get<PaginatedResponse<UserData>>(
      `${API_BASE_URL}/UserData/filter`,
      {
        params: { ...filters, page, pageSize },
      }
    );
    return response.data;
  },

  async searchUsers(
    searchOptions: SearchOptions,
    page = 1,
    pageSize = 10
  ): Promise<PaginatedResponse<UserData>> {
    const response = await axios.get<PaginatedResponse<UserData>>(
      `${API_BASE_URL}/UserData/search`,
      {
        params: { ...searchOptions, page, pageSize },
      }
    );
    return response.data;
  },
};
