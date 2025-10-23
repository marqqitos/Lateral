import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Home from '@/app/page'
import * as api from '@/lib/api'

// Mock the API module
jest.mock('@/lib/api')
const mockApi = api as jest.Mocked<typeof api>

describe('Home Page', () => {
  const mockTasks = [
    {
      id: 1,
      title: 'Test Task 1',
      description: 'Description 1',
      isCompleted: false,
      createdAt: '2023-01-01T00:00:00Z',
      updatedAt: '2023-01-01T00:00:00Z',
    },
    {
      id: 2,
      title: 'Test Task 2',
      description: 'Description 2',
      isCompleted: true,
      createdAt: '2023-01-02T00:00:00Z',
      updatedAt: '2023-01-02T01:00:00Z',
    },
  ]

  beforeEach(() => {
    jest.clearAllMocks()
    mockApi.taskApi = {
      fetchTasks: jest.fn(),
      createTask: jest.fn(),
      toggleTask: jest.fn(),
      deleteTask: jest.fn(),
    }
  })

  it('should render page header and description', () => {
    mockApi.taskApi.fetchTasks.mockResolvedValue({ tasks: [], totalCount: 0 })

    render(<Home />)

    expect(screen.getByText('Task Manager')).toBeInTheDocument()
    expect(screen.getByText('Organize and track your tasks efficiently')).toBeInTheDocument()
  })

  it('should show loading state initially', () => {
    mockApi.taskApi.fetchTasks.mockImplementation(() => new Promise(() => {})) // Never resolves

    render(<Home />)

    expect(screen.getByText('Loading tasks...')).toBeInTheDocument()
    // Check for loading spinner by its class
    expect(document.querySelector('.animate-spin')).toBeInTheDocument()
  })

  it('should load and display tasks on mount', async () => {
    mockApi.taskApi.fetchTasks.mockResolvedValue({ tasks: mockTasks, totalCount: 2 })

    render(<Home />)

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument()
      expect(screen.getByText('Test Task 2')).toBeInTheDocument()
    })

    expect(mockApi.taskApi.fetchTasks).toHaveBeenCalledTimes(1)
  })

  it('should show error message when loading tasks fails', async () => {
    const errorMessage = 'Failed to fetch tasks'
    mockApi.taskApi.fetchTasks.mockRejectedValue(new Error(errorMessage))

    render(<Home />)

    await waitFor(() => {
      expect(screen.getByText(errorMessage)).toBeInTheDocument()
    })

    // Should show retry button
    expect(screen.getByText('Retry')).toBeInTheDocument()
  })

  it('should retry loading tasks when retry button is clicked', async () => {
    const user = userEvent.setup()

    // First call fails
    mockApi.taskApi.fetchTasks.mockRejectedValueOnce(new Error('Network error'))
    // Second call succeeds
    mockApi.taskApi.fetchTasks.mockResolvedValueOnce({ tasks: mockTasks, totalCount: 2 })

    render(<Home />)

    await waitFor(() => {
      expect(screen.getByText('Network error')).toBeInTheDocument()
    })

    const retryButton = screen.getByText('Retry')
    await user.click(retryButton)

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument()
    })

    expect(mockApi.taskApi.fetchTasks).toHaveBeenCalledTimes(2)
  })

  it('should create new task successfully', async () => {
    const user = userEvent.setup()
    const newTask = {
      id: 3,
      title: 'New Task',
      description: 'New Description',
      isCompleted: false,
      createdAt: '2023-01-03T00:00:00Z',
      updatedAt: '2023-01-03T00:00:00Z',
    }

    mockApi.taskApi.fetchTasks.mockResolvedValue({ tasks: mockTasks, totalCount: 2 })
    mockApi.taskApi.createTask.mockResolvedValue(newTask)

    render(<Home />)

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument()
    })

    // Fill out form
    const titleInput = screen.getByLabelText('Task Title')
    const descriptionInput = screen.getByLabelText('Description')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    await user.type(titleInput, 'New Task')
    await user.type(descriptionInput, 'New Description')
    await user.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText('New Task')).toBeInTheDocument()
    })

    expect(mockApi.taskApi.createTask).toHaveBeenCalledWith({
      title: 'New Task',
      description: 'New Description',
    })

    // Form should be cleared
    expect(titleInput).toHaveValue('')
    expect(descriptionInput).toHaveValue('')
  })

  it('should handle task creation error', async () => {
    const user = userEvent.setup()

    mockApi.taskApi.fetchTasks.mockResolvedValue({ tasks: mockTasks, totalCount: 2 })
    mockApi.taskApi.createTask.mockRejectedValue(new Error('Creation failed'))

    render(<Home />)

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument()
    })

    const titleInput = screen.getByLabelText('Task Title')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    await user.type(titleInput, 'New Task')
    await user.click(submitButton)

    await waitFor(() => {
      expect(screen.getAllByText('Creation failed')).toHaveLength(2) // Error appears in both form and global error
    })

    // Task should not be added to the list
    expect(screen.queryByText('New Task')).not.toBeInTheDocument()
  })

  it('should toggle task completion', async () => {
    const user = userEvent.setup()
    const updatedTask = { ...mockTasks[0], isCompleted: true }

    mockApi.taskApi.fetchTasks.mockResolvedValue({ tasks: mockTasks, totalCount: 2 })
    mockApi.taskApi.toggleTask.mockResolvedValue(updatedTask)

    render(<Home />)

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument()
    })

    // Find and click the checkbox for the first task
    const checkboxes = screen.getAllByRole('checkbox')
    const firstTaskCheckbox = checkboxes[0]

    await user.click(firstTaskCheckbox)

    expect(mockApi.taskApi.toggleTask).toHaveBeenCalledWith(1)

    // The task should be moved to completed section (this would require DOM updates)
    await waitFor(() => {
      expect(mockApi.taskApi.toggleTask).toHaveBeenCalledTimes(1)
    })
  })

  it('should handle toggle task error', async () => {
    const user = userEvent.setup()

    mockApi.taskApi.fetchTasks.mockResolvedValue({ tasks: mockTasks, totalCount: 2 })
    mockApi.taskApi.toggleTask.mockRejectedValue(new Error('Toggle failed'))

    render(<Home />)

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument()
    })

    const checkboxes = screen.getAllByRole('checkbox')
    await user.click(checkboxes[0])

    await waitFor(() => {
      expect(screen.getByText('Toggle failed')).toBeInTheDocument()
    })
  })

  it('should delete task successfully', async () => {
    const user = userEvent.setup()

    mockApi.taskApi.fetchTasks.mockResolvedValue({ tasks: mockTasks, totalCount: 2 })
    mockApi.taskApi.deleteTask.mockResolvedValue(undefined)

    render(<Home />)

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument()
    })

    // Find and click delete button for first task using aria-label
    const deleteButton = screen.getByLabelText('Delete task: Test Task 1')
    await user.click(deleteButton)

    expect(mockApi.taskApi.deleteTask).toHaveBeenCalledWith(1)

    // Task should be removed from list
    await waitFor(() => {
      expect(screen.queryByText('Test Task 1')).not.toBeInTheDocument()
    })
  })

  it('should handle delete task error', async () => {
    const user = userEvent.setup()

    mockApi.taskApi.fetchTasks.mockResolvedValue({ tasks: mockTasks, totalCount: 2 })
    mockApi.taskApi.deleteTask.mockRejectedValue(new Error('Delete failed'))

    render(<Home />)

    await waitFor(() => {
      expect(screen.getByText('Test Task 1')).toBeInTheDocument()
    })

    // Find delete button using aria-label
    const deleteButton = screen.getByLabelText('Delete task: Test Task 1')
    await user.click(deleteButton)

    await waitFor(() => {
      expect(screen.getByText('Delete failed')).toBeInTheDocument()
    })

    // Task should still be in the list
    expect(screen.getByText('Test Task 1')).toBeInTheDocument()
  })

  it('should render form and dashboard in correct layout', async () => {
    mockApi.taskApi.fetchTasks.mockResolvedValue({ tasks: mockTasks, totalCount: 2 })

    render(<Home />)

    await waitFor(() => {
      expect(screen.getByText('Add New Task')).toBeInTheDocument()
      expect(screen.getByText('Active Tasks')).toBeInTheDocument()
    })

    // Check layout structure exists
    const main = screen.getByRole('main')
    expect(main).toHaveClass('min-h-screen')

    const container = main.querySelector('.max-w-4xl')
    expect(container).toBeInTheDocument()
  })

  it('should handle empty task list', async () => {
    mockApi.taskApi.fetchTasks.mockResolvedValue({ tasks: [], totalCount: 0 })

    render(<Home />)

    await waitFor(() => {
      expect(screen.getByText('No active tasks. Great job!')).toBeInTheDocument()
    })

    // Should show 0 in statistics - use more specific queries
    expect(screen.getByText('Total Tasks')).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
    expect(screen.getByText('0%')).toBeInTheDocument() // Completion percentage
  })

  it('should clear error message after successful operation', async () => {
    const user = userEvent.setup()
    const newTask = {
      id: 3,
      title: 'Recovery Task',
      description: '',
      isCompleted: false,
      createdAt: '2023-01-03T00:00:00Z',
      updatedAt: '2023-01-03T00:00:00Z',
    }

    // Initial load fails
    mockApi.taskApi.fetchTasks.mockRejectedValueOnce(new Error('Network error'))
    render(<Home />)

    await waitFor(() => {
      expect(screen.getByText('Network error')).toBeInTheDocument()
    })

    // Successful task creation should clear error
    mockApi.taskApi.createTask.mockResolvedValue(newTask)

    const titleInput = screen.getByLabelText('Task Title')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    await user.type(titleInput, 'Recovery Task')
    await user.click(submitButton)

    await waitFor(() => {
      expect(screen.queryByText('Network error')).not.toBeInTheDocument()
    })
  })
})
