import { AuthProvider } from './auth/AuthProvider'
import { NavBar } from './components/NavBar'
import './App.css'

function App() {
  return (
    <AuthProvider>
      <NavBar />
      <main>
        {/* Routes will be added in Task Group C */}
      </main>
    </AuthProvider>
  )
}

export default App
