# Add Feature Plan

Dokumen ini menjadi rencana kerja untuk menambahkan modul HR baru secara bertahap, dengan pola yang konsisten terhadap module existing:
- DTO + validator
- service
- controller
- entity + EF configuration
- unit test
- integration test
- migration dan seed data bila diperlukan

## Scope Modul

Fitur yang akan ditambahkan:
- Attendance
- Leave Master / Jenis Cuti
- Leave Request / Pengajuan Cuti
- Jatah Cuti

## Prinsip Implementasi

- Ikuti pattern module existing seperti `Employees`, `Positions`, `Departments`, dan `Roles`.
- Gunakan `Response<T>` dan `PagedResult<T>` untuk API contract yang konsisten.
- Gunakan `HasPermission` pada controller.
- Gunakan validation sebelum business logic.
- Tambahkan relasi EF, konfigurasi tabel, seed data, dan migration yang sesuai.
- Tambahkan unit test untuk service + validator dan integration test untuk endpoint utama.

## Phased Delivery

### Phase 1: Leave Master

Tujuan:
- Menyediakan master data jenis cuti sebagai dependensi untuk request cuti dan jatah cuti.

Deliverables:
- Entity `LeaveMaster`
- DTO request/query/response
- Validator
- Service + controller
- EF configuration
- Permission setup
- Unit test + integration test

Acceptance criteria:
- Admin bisa create/read/update/delete jenis cuti.
- Nama dan kode jenis cuti tidak boleh duplikat.
- Status jenis cuti bisa diaktifkan/nonaktifkan.

### Phase 2: Jatah Cuti

Tujuan:
- Menyimpan kuota cuti per employee, jenis cuti, dan tahun.

Deliverables:
- Entity `LeaveAllowance`
- DTO request/query/response
- Validator
- Service + controller
- EF configuration
- Unit test + integration test

Acceptance criteria:
- Admin bisa manage jatah cuti per employee per tahun.
- Kombinasi employee + jenis cuti + tahun tidak boleh duplikat.
- Data jatah cuti bisa dipakai sebagai referensi untuk leave request.

### Phase 3: Leave Request

Tujuan:
- Menyediakan flow pengajuan cuti dari employee.

Deliverables:
- Entity `LeaveRequest`
- Auto-generated request number
- DTO request/query/response
- Validator
- Service + controller
- EF configuration
- Unit test + integration test

Acceptance criteria:
- Pengajuan cuti memvalidasi relasi employee, jenis cuti, dan tanggal.
- Nomor pengajuan dibentuk otomatis.
- Lampiran bersifat opsional.
- Leave request bisa dilacak statusnya.

### Phase 4: Attendance

Tujuan:
- Mencatat kehadiran harian employee.

Deliverables:
- Entity `Attendance`
- DTO request/query/response
- Validator
- Service + controller
- EF configuration
- Unit test + integration test

Acceptance criteria:
- Attendance menyimpan tanggal, employee, jam masuk, jam keluar, dan status.
- Status attendance diturunkan dari jam masuk terhadap jam standar atau aturan yang disepakati di service.
- Data attendance bisa difilter dan diurutkan.

## Urutan Implementasi Detail

1. Tambahkan entity dan konfigurasi EF untuk seluruh modul baru.
2. Tambahkan `DbSet` pada `AppDbContext`.
3. Tambahkan permission baru dan seed awal.
4. Implementasikan service, controller, DTO, dan validator per fase.
5. Tambahkan unit test dan integration test per fase.
6. Buat migration untuk skema database.

## Catatan Teknis

- Jika ada aturan bisnis yang belum final, default dulu ke implementasi yang paling aman dan mudah diuji.
- Relasi yang wajib harus divalidasi di service sebelum `SaveChangesAsync`.
- Field opsional harus tetap tervalidasi di layer DTO/validator untuk menjaga API contract.

