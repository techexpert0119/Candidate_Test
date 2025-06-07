import React, { useState, useEffect } from "react";
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Box,
  TextField,
  Button,
  Skeleton,
  Typography,
} from "@mui/material";
import type { UserData } from "../types/userData";

interface UserDataTableProps {
  users: UserData[];
  totalCount: number;
  page: number;
  rowsPerPage: number;
  onPageChange: (newPage: number) => void;
  onRowsPerPageChange: (newRowsPerPage: number) => void;
  isLoading: boolean;
}

const UserDataTable: React.FC<UserDataTableProps> = ({
  users,
  totalCount,
  page,
  rowsPerPage,
  onPageChange,
  onRowsPerPageChange,
  isLoading,
}) => {
  const [pageInput, setPageInput] = useState<string>(page.toString());

  useEffect(() => {
    setPageInput(page.toString());
  }, [page]);

  const handlePageInputSubmit = () => {
    const newPage = parseInt(pageInput) - 1;
    if (
      !isNaN(newPage) &&
      newPage >= 0 &&
      newPage < Math.ceil(totalCount / rowsPerPage)
    ) {
      onPageChange(newPage);
    } else {
      setPageInput(page.toString());
    }
  };

  const handlePageInputChange = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    setPageInput(event.target.value);
  };

  const handleKeyPress = (event: React.KeyboardEvent) => {
    if (event.key === "Enter") {
      handlePageInputSubmit();
    }
  };

  const calculateRowNumber = (index: number) => {
    return (page - 1) * rowsPerPage + index + 1;
  };

  const renderSkeletonRows = () => {
    return Array(rowsPerPage)
      .fill(0)
      .map((_, index) => (
        <TableRow key={index}>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
          <TableCell>
            <Skeleton animation="wave" />
          </TableCell>
        </TableRow>
      ));
  };

  return (
    <Paper sx={{ width: "100%", overflow: "hidden" }}>
      <TableContainer sx={{ maxHeight: 440 }}>
        <Table stickyHeader aria-label="sticky table">
          <TableHead>
            <TableRow>
              <TableCell
                key="no"
                align="left"
                style={{ minWidth: 50, fontWeight: "bold" }}
              >
                No.
              </TableCell>
              <TableCell>First Name</TableCell>
              <TableCell>Last Name</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Gender</TableCell>
              <TableCell>Country</TableCell>
              <TableCell>Registration Date</TableCell>
              <TableCell>Birth Date</TableCell>
              <TableCell>Salary</TableCell>
              <TableCell>Title</TableCell>
              <TableCell>Comments</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {isLoading ? (
              renderSkeletonRows()
            ) : users.length === 0 ? (
              <TableRow>
                <TableCell colSpan={12} align="center">
                  No data available
                </TableCell>
              </TableRow>
            ) : (
              users.map((user, index) => (
                <TableRow hover role="checkbox" tabIndex={-1} key={index}>
                  <TableCell align="left">
                    {calculateRowNumber(index)}
                  </TableCell>
                  <TableCell>{user.firstName}</TableCell>
                  <TableCell>{user.lastName}</TableCell>
                  <TableCell>{user.email}</TableCell>
                  <TableCell>{user.gender}</TableCell>
                  <TableCell>{user.country}</TableCell>
                  <TableCell>
                    {new Date(user.registrationDate).toLocaleDateString()}
                  </TableCell>
                  <TableCell>
                    {new Date(user.birthDate).toLocaleDateString()}
                  </TableCell>
                  <TableCell>${user.salary.toLocaleString()}</TableCell>
                  <TableCell>{user.title}</TableCell>
                  <TableCell>{user.comments}</TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
      <Box
        sx={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          p: 2,
        }}
      >
        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
          <Typography variant="body2" color="text.secondary">
            Page {page} of {Math.ceil(totalCount / rowsPerPage)}
          </Typography>
          <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
            <TextField
              size="small"
              value={pageInput}
              onChange={handlePageInputChange}
              onKeyPress={handleKeyPress}
              sx={{ width: "60px" }}
            />
            <Button
              variant="contained"
              size="small"
              onClick={handlePageInputSubmit}
            >
              Go
            </Button>
          </Box>
        </Box>
        <TablePagination
          component="div"
          count={totalCount}
          page={page - 1}
          onPageChange={(_event, newPage) => onPageChange(newPage + 1)}
          rowsPerPage={rowsPerPage}
          onRowsPerPageChange={(event) =>
            onRowsPerPageChange(parseInt(event.target.value, 10))
          }
          rowsPerPageOptions={[5, 10, 25, 50]}
          disabled={isLoading}
          showFirstButton
          showLastButton
          labelDisplayedRows={({ from, to, count }) =>
            `${from}-${to} of ${count}`
          }
        />
      </Box>
    </Paper>
  );
};

export default UserDataTable;
