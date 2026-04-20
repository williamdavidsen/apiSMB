import { spawn } from 'node:child_process'
import { existsSync } from 'node:fs'
import net from 'node:net'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const scriptDir = path.dirname(fileURLToPath(import.meta.url))
const dashboardRoot = path.resolve(scriptDir, '..')
const repoRoot = path.resolve(dashboardRoot, '..', '..')
const apiProject = path.join(repoRoot, 'API', 'SecurityAssessmentAPI.csproj')
const apiPort = 1071
const viteCli = path.join(
  dashboardRoot,
  'node_modules',
  'vite',
  'bin',
  'vite.js',
)

const children = []
let shuttingDown = false

function startProcess(label, command, args, cwd) {
  const child = spawn(command, args, {
    cwd,
    env: {
      ...process.env,
      FORCE_COLOR: '1',
    },
    stdio: 'inherit',
  })

  children.push(child)

  child.on('error', (error) => {
    console.error(`[${label}] failed to start: ${error.message}`)
    shutdown(1)
  })

  child.on('exit', (code, signal) => {
    if (shuttingDown) return

    const reason = signal ? `signal ${signal}` : `code ${code ?? 0}`
    console.error(`[${label}] exited with ${reason}`)
    shutdown(code ?? 1)
  })

  return child
}

function stopProcess(child) {
  if (!child.pid || child.killed) return

  if (process.platform === 'win32') {
    spawn('taskkill', ['/pid', String(child.pid), '/t', '/f'], {
      stdio: 'ignore',
    })
    return
  }

  child.kill('SIGTERM')
}

function shutdown(code = 0) {
  if (shuttingDown) return
  shuttingDown = true

  for (const child of children) {
    stopProcess(child)
  }

  setTimeout(() => {
    process.exit(code)
  }, 500)
}

function isPortOpen(port) {
  return new Promise((resolve) => {
    const socket = net.createConnection({ host: '127.0.0.1', port })

    socket.once('connect', () => {
      socket.end()
      resolve(true)
    })

    socket.once('error', () => {
      resolve(false)
    })

    socket.setTimeout(1000, () => {
      socket.destroy()
      resolve(false)
    })
  })
}

process.on('SIGINT', () => shutdown(0))
process.on('SIGTERM', () => shutdown(0))

if (!existsSync(apiProject)) {
  console.error(`API project not found: ${apiProject}`)
  process.exit(1)
}

if (!existsSync(viteCli)) {
  console.error('Vite binary not found. Run npm install in Frontend/dashboard first.')
  process.exit(1)
}

if (await isPortOpen(apiPort)) {
  console.log(`[api] http://localhost:${apiPort} is already running; reusing it.`)
} else {
  startProcess('api', 'dotnet', ['run', '--project', apiProject, '--launch-profile', 'http'], repoRoot)
}

startProcess('frontend', process.execPath, [viteCli], dashboardRoot)
