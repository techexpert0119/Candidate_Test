export interface UserData {
  firstName: string;
  lastName: string;
  email: string;
  title: string;
  gender: string;
  country: string;
  registrationDate: string;
  birthDate: string;
  salary: number;
  comments: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
}

export interface FilterOptions {
  gender?: string;
  country?: string;
  registrationDateFrom?: string;
  registrationDateTo?: string;
  birthDateFrom?: string;
  birthDateTo?: string;
  minSalary?: number;
  maxSalary?: number;
}

export interface SearchOptions {
  firstName?: string;
  lastName?: string;
  email?: string;
  title?: string;
  comments?: string;
}
